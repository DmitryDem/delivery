using DeliveryApp.Api;
using DeliveryApp.Core.Domain.Services;
using DeliveryApp.Core.Ports;
using DeliveryApp.Infrastructure.Adapters.Postgres;
using DeliveryApp.Infrastructure.Adapters.Postgres.Repositories;
using Microsoft.EntityFrameworkCore;
using Primitives;
using System.Reflection;
using DeliveryApp.Core.Application.UseCases.Commands.AssignOrderToCourier;
using DeliveryApp.Core.Application.UseCases.Commands.CreateOrder;
using DeliveryApp.Core.Application.UseCases.Commands.MoveCouriers;
using DeliveryApp.Core.Application.UseCases.Queries.GetBusyCouriers;
using DeliveryApp.Core.Application.UseCases.Queries.GetIncompletedOrders;

using MediatR;

var builder = WebApplication.CreateBuilder(args);

// Health Checks
builder.Services.AddHealthChecks();

// Cors
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.AllowAnyOrigin(); // Не делайте так в проде!
        });
});

// Configuration
builder.Services.ConfigureOptions<SettingsSetup>();
builder.Services.AddTransient<IDispatchService, DispatchService>();

var connectionString = builder.Configuration["CONNECTION_STRING"];
//var connectionString = "Host=localhost;Port=5432;Database=postgres;Username=username;Password=secret;";

// Database, ORM 
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString,
        sqlOptions => { sqlOptions.MigrationsAssembly("DeliveryApp.Infrastructure"); });
    options.EnableSensitiveDataLogging();
}
);

// UnitOfWork
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Repositories
builder.Services.AddTransient<IOrderRepository, OrderRepository>();
builder.Services.AddTransient<ICourierRepository, CourierRepository>();

// Mediator
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

// Commands
builder.Services.AddTransient<IRequestHandler<CreateOrderCommand, bool>, CreateOrderCommandHandler>();
builder.Services.AddTransient<IRequestHandler<MoveCouriersCommand, bool>, MoveCouriersCommandHandler>();
builder.Services.AddTransient<IRequestHandler<AssignOrderToCourierCommand, bool>, AssignOrderToCourierCommandHandler>();

// Queries

var app = builder.Build();

builder.Services.AddTransient<IRequestHandler<GetBusyCouriersQuery, GetBusyCouriersResponseModel>>(_ => new GetBusyCouriersQueryHandler(connectionString));
builder.Services.AddTransient<IRequestHandler<GetIncompletedOrdersQuery, GetIncompletedOrdersResponseModel>>(_ => new GetIncompletedOrdersQueryHandler(connectionString));

// -----------------------------------
// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();
else
    app.UseHsts();

app.UseHealthChecks("/health");
app.UseRouting();

// Apply Migrations
//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//    db.Database.Migrate();
//}
app.Run();