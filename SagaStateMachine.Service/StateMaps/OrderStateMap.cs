using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SagaStateMachine.Service.StateInstances;

namespace SagaStateMachine.Service.StateMaps;

public class OrderStateMap : SagaClassMap<OrderStateInstance>
{
    protected override void Configure(EntityTypeBuilder<OrderStateInstance> entity, ModelBuilder model)
    {
        entity.HasKey(x => x.CorrelationId);
        entity.Property(x => x.BuyerId).IsRequired();
        entity.Property(x => x.OrderId).IsRequired();
        entity.Property(x => x.TotalPrice).HasDefaultValue(0);
        entity.Property(x => x.CurrentState).IsRequired();
        entity.Property(x => x.CreatedDate).IsRequired();
    }
} 