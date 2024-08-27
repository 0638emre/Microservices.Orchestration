using MassTransit;
using Microsoft.EntityFrameworkCore;
using SagaStateMachine.Service.StateDbContext;
using SagaStateMachine.Service.StateInstances;
using SagaStateMachine.Service.StateMachines;
using Shared.Settings;

var builder = Host.CreateApplicationBuilder(args);
// builder.Services.AddHostedService<Worker>();

builder.Services.AddMassTransit(configurator =>
{
    configurator.AddSagaStateMachine<OrderStateMachine, OrderStateInstance>()
        .EntityFrameworkRepository(opt =>
        {
            opt.AddDbContext<DbContext, OrderStateDbContext>((provider, _builder) =>
            {
                _builder.UseSqlServer(builder.Configuration.GetConnectionString("OrchestrationOrderStateDB"));
            });
        });

    configurator.UsingRabbitMq((context, configure) =>
    {
        configure.Host("localhost", 5672, "/", h =>
        {
            h.Username("user");
            h.Password("password");
        });
        
        configure.ReceiveEndpoint(RabbitMQSettings.StateMachineQueue, e => e.ConfigureSaga<OrderStateInstance>(context));
    });
});

var host = builder.Build();
host.Run();