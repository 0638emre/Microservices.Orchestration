using MassTransit;
using Payment.API.Consumers;
using Shared.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(configurator =>
{
    configurator.AddConsumer<PaymentStartedEventConsumer>();
    configurator.UsingRabbitMq((context, configure) =>
    {
        configure.Host("localhost", 5672, "/", h =>
        {
            h.Username("user");
            h.Password("password");
        });
        
        configure.ReceiveEndpoint(RabbitMQSettings.Payment_StartedEventQueue, e=> 
            e.ConfigureConsumer<PaymentStartedEventConsumer>(context));
    });
});

var app = builder.Build();

app.Run();