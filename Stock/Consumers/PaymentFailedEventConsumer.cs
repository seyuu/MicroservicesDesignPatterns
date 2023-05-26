using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared;
using Stock.Models;

namespace Stock.Consumers
{
    public class PaymentFailedEventConsumer : IConsumer<PaymentFailedEvent> // nereyi dinlemek istiyorsak onu ekliyoruz
    {
        private readonly AppDbContext _context;
        private ILogger<PaymentFailedEventConsumer> _logger;
       
        public PaymentFailedEventConsumer(AppDbContext context, ILogger<PaymentFailedEventConsumer> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
        {
            var stockResult = new List<bool>();

            foreach (var item in context.Message.OrderItems)
            {
                var stock =await _context.Stocks.FirstOrDefaultAsync(x=> x.ProductId == item.ProductId);

                if (stock != null)
                {
                    stock.Count += item.Count;
                }
                await _context.SaveChangesAsync();
                _logger.LogInformation($"stock was released for buyer id : {context.Message.OrderId}");

            }
        }
    }
}
