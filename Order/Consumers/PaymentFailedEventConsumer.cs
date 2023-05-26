using MassTransit;
using Order.Models;
using Shared;

namespace Order.Consumers
{
    public class PaymentFailedEventConsumer : IConsumer<PaymentFailedEvent>
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PaymentFailedEventConsumer> _logger;

        public PaymentFailedEventConsumer(AppDbContext context, ILogger<PaymentFailedEventConsumer> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
        {
            var order = await _context.Orders.FindAsync(context.Message.OrderId);
            if (order != null)
            {
                order.Status = OrderStatus.Fail;
                order.FailMessage = context.Message.Message;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"order (id={context.Message.OrderId}) status changed : {order.Status}");
            }
            else
            {
                _logger.LogError($"order (Id={context.Message.OrderId}) not found");
            }
        }
    }
}
