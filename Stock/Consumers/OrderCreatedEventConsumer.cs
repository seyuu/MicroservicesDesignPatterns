using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared;
using Stock.Models;

namespace Stock.Consumers
{
    public class OrderCreatedEventConsumer : IConsumer<OrderCreatedEvent> // nereyi dinlemek istiyorsak onu ekliyoruz
    {
        private readonly AppDbContext _context;
        private ILogger<OrderCreatedEventConsumer> _logger;
        private readonly ISendEndpointProvider _sendEndpointProvider; // sadece ödeme servisi dinlediği için
        private readonly IPublishEndpoint _publishEndpoint;

        public OrderCreatedEventConsumer(AppDbContext context, ILogger<OrderCreatedEventConsumer> logger, ISendEndpointProvider sendEndpointProvider, IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _logger = logger;
            _sendEndpointProvider = sendEndpointProvider;
            _publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            var stockResult = new List<bool>();

            foreach (var item in context.Message.OrderItems)
            {
                stockResult.Add(await _context.Stocks.AnyAsync(x=> x.ProductId == item.ProductId && x.Count > item.Count));
            }

            if (stockResult.All(x=> x.Equals(true))) // tüm stoklar varsa 
            {
                foreach (var item in context.Message.OrderItems)
                {
                    var stock = await _context.Stocks.FirstOrDefaultAsync(x => x.ProductId == item.ProductId);

                    if(stock != null)
                    {
                        stock.Count-=item.Count;
                    }

                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation($"stock was reserved for buyer id : {context.Message.BuyerId}");

                var senddEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettings.StockReservedEventQueueName}"));

                StockReservedEvent stockReservedEvent = new StockReservedEvent()
                {
                    Payment = context.Message.Payment,
                    BuyerId = context.Message.BuyerId,
                    OrderId = context.Message.OrderId,
                    OrderItems = context.Message.OrderItems
                };

                await senddEndpoint.Send(stockReservedEvent);

            }
            else
            {
                await _publishEndpoint.Publish(new StockNotReservedEvent()
                {
                    OrderId = context.Message.OrderId,
                    Message = "not enough stock"
                });

                _logger.LogInformation($"not enough stock for buyer id : {context.Message.BuyerId}");
            }
        }
    }
}
