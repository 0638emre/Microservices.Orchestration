using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Order.API.Consumers;
using Order.API.Enums;
using Order.API.Models;
using Order.API.Models.Context;
using Order.API.ViewModels;
using Shared.Messages;
using Shared.OrderEvents;
using Shared.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(configurator =>
{
    configurator.AddConsumer<OrderCompletedEventConsumer>();
    configurator.AddConsumer<OrderFailedEventConsumer>();
    configurator.UsingRabbitMq((context, configure) =>
    {
        configure.Host("localhost", 5672, "/", h =>
        {
            h.Username("user");
            h.Password("password");
        });
        
        configure.ReceiveEndpoint(RabbitMQSettings.Order_OrderCompletedEventQueue, e => e.ConfigureConsumer<OrderCompletedEventConsumer>(context));
        configure.ReceiveEndpoint(RabbitMQSettings.Order_OrderFailedEventQueue, e => e.ConfigureConsumer<OrderFailedEventConsumer>(context));
    });
});

builder.Services.AddDbContext<OrderAPIDBContext>(ops =>
    ops.UseSqlServer(builder.Configuration.GetConnectionString("OrderAPIDB")));

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/create-order", async ([FromBody] CreateOrderVM model, OrderAPIDBContext context, ISendEndpointProvider sendEndpointProvider) =>
{
    Order.API.Models.Order order = new()
    {
        BuyerId = model.BuyerId,
        OrderStatus = OrderStatus.Suspend,
        TotalPrice = model.OrderItems.Sum(oi => oi.Count * oi.Price),
        OrderItems = model.OrderItems.Select(o => new OrderItem()
        {
            ProductId = o.ProductId,
            Count = o.Count,
            Price = o.Price
        }).ToList(),
    };
    
    await context.Orders.AddAsync(order);
    await context.SaveChangesAsync();

    OrderStartedEvent orderStartedEvent = new()
    {
        BuyerId = order.BuyerId,
        OrderId = order.Id,
        TotalPrice = order.TotalPrice,
        OrderItems = order.OrderItems.Select(oi => new OrderItemMessage()
        {
            ProductId = oi.ProductId,
            Price = oi.Price,
            Count = oi.Count
        }).ToList()
    };
    
   var sendEndpoint =  await sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettings.StateMachineQueue}"));
   
   await sendEndpoint.Send(orderStartedEvent);
   
});

app.Run();