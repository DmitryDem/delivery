using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using DeliveryApp.Infrastructure.Adapters.Postgres;
using DeliveryApp.Infrastructure.Adapters.Postgres.Repositories;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;

using NSubstitute;

using Testcontainers.PostgreSql;
using Xunit;

namespace DeliveryApp.IntegrationTests.Repositories
{
    public class CourierRepositoryTestsShould : IAsyncLifetime
    {
        /// <summary>
        /// Контейнер с PostgreSQL
        /// </summary>
        private readonly PostgreSqlContainer postgreSqlContainer = new PostgreSqlBuilder()
            .WithImage("postgres:14.7")
            .WithDatabase("delivery")
            .WithUsername("username")
            .WithPassword("secret")
            .WithCleanUp(true)
            .Build();

        private ApplicationDbContext context;
        private IMediator mediator;

        /// <summary>
        /// Инициализация контейнера
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task InitializeAsync()
        {
            await this.postgreSqlContainer.StartAsync();

            var contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(
                    this.postgreSqlContainer.GetConnectionString(),
                    sqlOptions => { sqlOptions.MigrationsAssembly("DeliveryApp.Infrastructure"); })
                .Options;

            this.context = new ApplicationDbContext(contextOptions);
            this.mediator = Substitute.For<IMediator>();
            this.context.Database.Migrate();
        }

        /// <summary>
        /// Очистка контейнера
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task DisposeAsync()
        {
            await this.postgreSqlContainer.DisposeAsync().AsTask();
        }

        [Fact]
        public async Task CanAddCourier()
        {
            //Arrange 
            var location = Location.CreateRandom().Value;
            var courier = Courier.Create("Alex", "Car", 3, location).Value;

            //Act
            var courierRepository = new CourierRepository(this.context);
            await courierRepository.AddAsync(courier);
            var unitOfWork = new UnitOfWork(this.context, this.mediator);
            await unitOfWork.SaveChangesAsync();

            //Assert
            var courierFromDb = await courierRepository.GetAsync(courier.Id);
            courier.Should().BeEquivalentTo(courierFromDb);
        }

        [Fact]
        public async Task CanUpdateCourier()
        {
            //Arrange 
            var location = Location.CreateRandom().Value;
            var courier = Courier.Create("Alex", "Car", 3, location).Value;
            var courierRepository = new CourierRepository(this.context);
            await courierRepository.AddAsync(courier);
            var unitOfWork = new UnitOfWork(this.context, this.mediator);
            await unitOfWork.SaveChangesAsync();

            //Act
            courier.SetBusy();
            await unitOfWork.SaveChangesAsync();

            //Assert
            var courierFromDb = await courierRepository.GetAsync(courier.Id);
            courier.Status.Should().Be(courierFromDb.Status);
        }

        [Fact]
        public async Task ReturnCourierById()
        {
            //Arrange 
            var location = Location.CreateRandom().Value;
            var courier = Courier.Create("Alex", "Car", 3, location).Value;
            var courierRepository = new CourierRepository(this.context);
            await courierRepository.AddAsync(courier);
            var unitOfWork = new UnitOfWork(this.context, this.mediator);
            await unitOfWork.SaveChangesAsync();

            //Act
            var courierFromDb = await courierRepository.GetAsync(courier.Id);

            //Assert
            courierFromDb.Should().BeEquivalentTo(courier);
        }

        [Fact]
        public async Task ReturnAllInFreeStatus()
        {
            //Arrange
            var courierRepository = new CourierRepository(this.context);
            var unitOfWork = new UnitOfWork(this.context, this.mediator);

            var courier1 = Courier.Create("Alex", "Car", 3, Location.CreateRandom().Value).Value;
            await courierRepository.AddAsync(courier1);

            var courier2 = Courier.Create("Mark", "Bike", 2, Location.CreateRandom().Value).Value;
            await courierRepository.AddAsync(courier2);

            var courier3 = Courier.Create("Ben", "Foot", 1, Location.CreateRandom().Value).Value;
            await courierRepository.AddAsync(courier3);

            await unitOfWork.SaveChangesAsync();

            courier2 = await courierRepository.GetAsync(courier2.Id);
            courier2.SetBusy();

            await unitOfWork.SaveChangesAsync();

            //Act
            var couriersFromDb = await courierRepository.GetAllInFreeStatus();

            //Assert
            couriersFromDb.Should().HaveCount(2);
            couriersFromDb.Should().Contain(o => o.Id == courier1.Id);
            couriersFromDb.Should().Contain(o => o.Id == courier3.Id);
        }
    }
}
