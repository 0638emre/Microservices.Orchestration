using MassTransit;
using Shared.Messages;

namespace Shared.OrderEvents;

public class OrderCreatedEvent : CorrelatedBy<Guid>
{
    public OrderCreatedEvent(Guid correlationId)
    {
        CorrelationId = correlationId;
    }
    
    // public int OrderId { get; set; }
    public Guid CorrelationId { get; }
    public List<OrderItemMessage> OrderItems { get; set; }
    
}