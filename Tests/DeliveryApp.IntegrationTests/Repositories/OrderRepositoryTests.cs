using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Infrastructure.Adapters.Postgres;
using DeliveryApp.Infrastructure.Adapters.Postgres.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;
using Location = DeliveryApp.Core.Domain.Model.SharedKernel.Location;

namespace DeliveryApp.IntegrationTests.Repositories
{
    public class OrderRepositoryShould : IAsyncLifetime
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
                    sqlOptions => { sqlOptions.MigrationsAssembly("DeliveryApp.Infrastructure");})
                .Options;

            this.context = new ApplicationDbContext(contextOptions);
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
        public async Task CanAddOrder()
        {
            //Arrange 
            var basketId = Guid.NewGuid();
            var location = Location.CreateRandom().Value;
            var order = Order.Create(basketId, location).Value;

            //Act
            var orderRepository = new OrderRepository(this.context);
            await orderRepository.AddAsync(order);
            var unitOfWork = new UnitOfWork(this.context);
            await unitOfWork.SaveChangesAsync();

            //Assert
            var orderFromDb = await orderRepository.GetAsync(order.Id);
            order.Should().BeEquivalentTo(orderFromDb);
        }

        [Fact]
        public async Task CanUpdateOrder()
        {
            //Arrange 
            var basketId = Guid.NewGuid();
            var location = Location.CreateRandom().Value;
            var order = Order.Create(basketId, location).Value;
            var orderRepository = new OrderRepository(this.context);
            await orderRepository.AddAsync(order);
            var unitOfWork = new UnitOfWork(this.context);
            await unitOfWork.SaveChangesAsync();

            //Act
            var courier = Courier.Create("Test", "Test", 1, Location.CreateRandom().Value).Value;
            order.Assignee(courier);
            await unitOfWork.SaveChangesAsync();

            //Assert
            var orderFromDb = await orderRepository.GetAsync(order.Id);
            orderFromDb.CourierId.Should().Be(courier.Id);
            orderFromDb.Status.Should().Be(OrderStatus.Assign());
        }

        [Fact]
        public async Task ReturnOrderById()
        {
            //Arrange 
            var basketId = Guid.NewGuid();
            var location = Location.CreateRandom().Value;
            var order = Order.Create(basketId, location).Value;
            var orderRepository = new OrderRepository(this.context);
            await orderRepository.AddAsync(order);
            var unitOfWork = new UnitOfWork(this.context);
            await unitOfWork.SaveChangesAsync();

            //Act
            var orderFromDb = await orderRepository.GetAsync(order.Id);

            //Assert
            orderFromDb.Should().BeEquivalentTo(order);
        }

        [Fact]
        public async Task ReturnFirstInCreatedStatus()
        {
            //Arrange 
            var order1 = Order.Create(Guid.NewGuid(), Location.CreateRandom().Value).Value;

            var orderRepository = new OrderRepository(this.context);
            await orderRepository.AddAsync(order1);

            var order2 = Order.Create(Guid.NewGuid(), Location.CreateRandom().Value).Value;
            await orderRepository.AddAsync(order2);

            var unitOfWork = new UnitOfWork(this.context);
            await unitOfWork.SaveChangesAsync();

            var orderFromDb = await orderRepository.GetAsync(order1.Id);
            orderFromDb.Assignee(Courier.Create("Test", "Test", 1, Location.CreateRandom().Value).Value);
            await unitOfWork.SaveChangesAsync();

            //Act
            orderFromDb = await orderRepository.GetFirstInCreatedStatusAsync();

            //Assert
            orderFromDb.Should().BeEquivalentTo(order2);
        }

        [Fact]
        public async Task ReturnAllInAssignStatus()
        {
            //Arrange
            var orderRepository = new OrderRepository(this.context);
            var unitOfWork = new UnitOfWork(this.context);

            var order1 = Order.Create(Guid.NewGuid(), Location.CreateRandom().Value).Value;
            await orderRepository.AddAsync(order1);

            var order2 = Order.Create(Guid.NewGuid(), Location.CreateRandom().Value).Value;
            await orderRepository.AddAsync(order2);

            var order3 = Order.Create(Guid.NewGuid(), Location.CreateRandom().Value).Value;
            await orderRepository.AddAsync(order3);

            await unitOfWork.SaveChangesAsync();

            order1 = await orderRepository.GetAsync(order1.Id);
            order3 = await orderRepository.GetAsync(order3.Id);
            order1.Assignee(Courier.Create("Test1", "Test1", 1, Location.CreateRandom().Value).Value);
            order3.Assignee(Courier.Create("Test2", "Test2", 2, Location.CreateRandom().Value).Value);

            await unitOfWork.SaveChangesAsync();

            //Act
            var ordersFromDb = await orderRepository.GetAllInAssignStatus();

            //Assert
            ordersFromDb.Should().HaveCount(2);
            ordersFromDb.Should().Contain(o => o.Id == order1.Id);
            ordersFromDb.Should().Contain(o => o.Id == order3.Id);
        }
    }
}
