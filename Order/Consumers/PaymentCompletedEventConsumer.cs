﻿using MassTransit;
using Order.Models;
using Shared;

namespace Order.Consumers
{
    public class PaymentCompletedEventConsumer : IConsumer<PaymentCompletedEvent>
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PaymentCompletedEventConsumer> _logger;

        public PaymentCompletedEventConsumer(AppDbContext context, ILogger<PaymentCompletedEventConsumer> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<PaymentCompletedEvent> context)
        {
            var order = await _context.Orders.FindAsync(context.Message.OrderId);
            if (order != null)
            {
                order.Status = OrderStatus.Complete;
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
