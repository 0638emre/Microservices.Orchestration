using MassTransit;
using MongoDB.Driver;
using Shared.Settings;
using Stock.API.Consumers;
using Stock.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(configurator =>
{
    configurator.AddConsumer<OrderCreatedEventConsumer>();
    configurator.AddConsumer<StockRollbackMessageConsumer>();
    configurator.UsingRabbitMq((context, configure) =>
    {
        configure.Host("localhost", 5672, "/", h =>
        {
            h.Username("user");
            h.Password("password");
        });
        
        configure.ReceiveEndpoint(RabbitMQSettings.Stock_OrderCreatedEventQueue, e => e.ConfigureConsumer<OrderCreatedEventConsumer>(context));
        
        configure.ReceiveEndpoint(RabbitMQSettings.Stock_RollbackMessageQueue, e => e.ConfigureConsumer<StockRollbackMessageConsumer>(context));
    });
});

builder.Services.AddSingleton<MongoDBServices>();


var app = builder.Build();

using var scope = builder.Services.BuildServiceProvider().CreateScope();
var mongoDbService = scope.ServiceProvider.GetRequiredService<MongoDBServices>();
if (!await (await mongoDbService.GetCollection<Stock.API.Models.Stock>().FindAsync(x => true)).AnyAsync())
{
    mongoDbService.GetCollection<Stock.API.Models.Stock>().InsertOne(new Stock.API.Models.Stock()
    {
        ProductId = 1,
        Count = 1000
    }); 
    mongoDbService.GetCollection<Stock.API.Models.Stock>().InsertOne(new Stock.API.Models.Stock()
    {
        ProductId = 2,
        Count = 2000
    });
    mongoDbService.GetCollection<Stock.API.Models.Stock>().InsertOne(new Stock.API.Models.Stock()
    {
        ProductId = 3,
        Count = 1400
    });
    mongoDbService.GetCollection<Stock.API.Models.Stock>().InsertOne(new Stock.API.Models.Stock()
    {
        ProductId = 4,
        Count = 3000
    }); mongoDbService.GetCollection<Stock.API.Models.Stock>().InsertOne(new Stock.API.Models.Stock()
    {
        ProductId = 5,
        Count = 3400
    });
}
app.Run();