using MassTransit;
using Order.API.Enums;
using Order.API.Models.Context;
using Shared.OrderEvents;

namespace Order.API.Consumers;

public class OrderFailedEventConsumer(OrderAPIDBContext dbContext) : IConsumer<OrderFailedEvent>
{
    public async Task Consume(ConsumeContext<OrderFailedEvent> context)
    {
        Models.Order order =  await dbContext.Orders.FindAsync(context.Message.OrderId);

        if (order is not null)
        {
            order.OrderStatus = OrderStatus.Failed;
        }
       
        await dbContext.SaveChangesAsync();
    }
}