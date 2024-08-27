using MassTransit;
using Order.API.Enums;
using Order.API.Models.Context;
using Shared.OrderEvents;

namespace Order.API.Consumers;

public class OrderCompletedEventConsumer(OrderAPIDBContext dbContext) : IConsumer<OrderCompletedEvent>
{
    public async Task Consume(ConsumeContext<OrderCompletedEvent> context)
    {
       Models.Order order =  await dbContext.Orders.FindAsync(context.Message.OrderId);

       if (order is not null)
       {
           order.OrderStatus = OrderStatus.Completed;
       }
       
       await dbContext.SaveChangesAsync();
    }
}