using DeliveryApp.Api;
using DeliveryApp.Core.Domain.Services;
using DeliveryApp.Core.Ports;
using DeliveryApp.Infrastructure.Adapters.Postgres;
using DeliveryApp.Infrastructure.Adapters.Postgres.Repositories;
using Microsoft.EntityFrameworkCore;
using Primitives;
using System.Reflection;
using CSharpFunctionalExtensions;

using DeliveryApp.Api.Adapters.Jobs;
using DeliveryApp.Core.Application.UseCases.Commands.AssignOrderToCourier;
using DeliveryApp.Core.Application.UseCases.Commands.CreateCourier;
using DeliveryApp.Core.Application.UseCases.Commands.CreateOrder;
using DeliveryApp.Core.Application.UseCases.Commands.MoveCouriers;
using DeliveryApp.Core.Application.UseCases.Queries.GetBusyCouriers;
using DeliveryApp.Core.Application.UseCases.Queries.GetIncompletedOrders;
using MediatR;
using Microsoft.OpenApi.Models;
using OpenApi.Filters;
using OpenApi.OpenApi;
using DeliveryApp.Core.Application.UseCases.Queries;
using DeliveryApp.Core.Application.UseCases.Queries.GetAllCouriers;
using DeliveryApp.Core.Domain.Model.CourierAggregate;

using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

using OpenApi.Formatters;

using Quartz;

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

//var connectionString = builder.Configuration["CONNECTION_STRING"];
var connectionString = "Host=localhost;Port=5432;Database=delivery;Username=username;Password=secret;";

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
builder.Services.AddTransient<IRequestHandler<CreateOrderCommand, UnitResult<Error>>, CreateOrderCommandHandler>();
builder.Services.AddTransient<IRequestHandler<CreateCourierCommand, UnitResult<Error>>, CreateCourierCommandHandler>();
builder.Services.AddTransient<IRequestHandler<MoveCouriersCommand, UnitResult<Error>>, MoveCouriersCommandHandler>();
builder.Services.AddTransient<IRequestHandler<AssignOrderToCourierCommand, UnitResult<Error>>, AssignOrderToCourierCommandHandler>();

// Queries
builder.Services.AddTransient<IRequestHandler<GetBusyCouriersQuery, GetCouriersResponseModel>>(_ => new GetBusyCouriersQueryHandler(connectionString));
builder.Services.AddTransient<IRequestHandler<GetAllCouriersQuery, GetCouriersResponseModel>>(_ => new GetAllCouriersQueryHandler(connectionString));
builder.Services.AddTransient<IRequestHandler<GetIncompletedOrdersQuery, GetIncompletedOrdersResponseModel>>(_ => new GetIncompletedOrdersQueryHandler(connectionString));

// HTTP Controllers
builder.Services.AddControllers(options => { options.InputFormatters.Insert(0, new InputFormatterStream()); })
    .AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            options.SerializerSettings.Converters.Add(new StringEnumConverter
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            });
        });

// Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("1.0.0", new OpenApiInfo
    {
        Title = "Delivery Service",
        Description = "Отвечает за доставку заказа",
        Contact = new OpenApiContact
        {
            Name = "Kirill Vetchinkin",
            Url = new Uri("https://microarch.ru"),
            Email = "info@microarch.ru"
        }
    });
    options.CustomSchemaIds(type => type.FriendlyId(true));
    options.IncludeXmlComments(
        $"{AppContext.BaseDirectory}{Path.DirectorySeparatorChar}{Assembly.GetEntryAssembly()?.GetName().Name}.xml");
    options.DocumentFilter<BasePathFilter>("");
    options.OperationFilter<GeneratePathParamsValidationFilter>();
});
builder.Services.AddSwaggerGenNewtonsoftSupport();

// Quartz
builder.Services.AddQuartz(configure =>
    {
        var assignOrdersJobKey = new JobKey(nameof(AssignOrdersJob));
        var moveCouriersJobKey = new JobKey(nameof(MoveCouriersJob));
        configure
            .AddJob<AssignOrdersJob>(assignOrdersJobKey)
            .AddTrigger(
                trigger => trigger.ForJob(assignOrdersJobKey)
                    .WithSimpleSchedule(
                        schedule => schedule.WithIntervalInSeconds(1)
                            .RepeatForever()))
            .AddJob<MoveCouriersJob>(moveCouriersJobKey)
            .AddTrigger(
                trigger => trigger.ForJob(moveCouriersJobKey)
                    .WithSimpleSchedule(
                        schedule => schedule.WithIntervalInSeconds(2)
                            .RepeatForever()));
    });
builder.Services.AddQuartzHostedService();

var app = builder.Build();

// -----------------------------------
// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();
else
    app.UseHsts();

app.UseHealthChecks("/health");
app.UseRouting();
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseSwagger(c => { c.RouteTemplate = "openapi/{documentName}/openapi.json"; })
    .UseSwaggerUI(options =>
        {
            options.RoutePrefix = "openapi";
            options.SwaggerEndpoint("/openapi/1.0.0/openapi.json", "Swagger Delivery Service");
            options.RoutePrefix = string.Empty;
            options.SwaggerEndpoint("/openapi-original.json", "Swagger Delivery Service");
        });

app.UseCors();
app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

// Apply Migrations
//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//    db.Database.Migrate();
//}

app.Run();