using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Order.DTOs;
using Order.Models;
using Shared;

namespace Order.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint;

        public OrdersController(AppDbContext context, IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
        }

        [HttpPost]
        public async Task<IActionResult> Create(OrderCreateDto orderCreate)
        {
            var newOrder = new Models.Order
            {
                BuyerId = orderCreate.BuyerId,
                Status = OrderStatus.Suspend,
                Address = new Address
                {
                    Line = orderCreate.Address.Line,
                    Province = orderCreate.Address.Province,
                    District = orderCreate.Address.District
                },
                CreatedDate = DateTime.Now
            };

            orderCreate.OrderItems.ForEach(item =>
            {
                newOrder.Items.Add(new OrderItem()
                {
                    Price = item.Price,
                    ProductId = item.ProductId,
                    Count = item.Count
                });
            });

            await _context.AddAsync(newOrder);
            await _context.SaveChangesAsync();


            var orderCreatedEvent = new OrderCreatedEvent()
            {
                BuyerId = orderCreate.BuyerId,
                OrderId = newOrder.Id,
                Payment = new PaymentMessage
                {
                    CardName = orderCreate.Payment.CardName,
                    CardNumber = orderCreate.Payment.CardNumber,
                    Expiration = orderCreate.Payment.Expiration,
                    CVV = orderCreate.Payment.CVV,
                    TotalPrice = orderCreate.OrderItems.Sum(item => item.Price * item.Count)
                }
            };

            orderCreate.OrderItems.ForEach(item =>
            {
                orderCreatedEvent.OrderItems.Add(new OrderItemMessage
                {
                    Count = item.Count,
                    ProductId = item.ProductId
                });
            });

            // direkt exchange e gider. subscribe olmadan alamazlar 
            // farklı farklı srvisler dinleyebilecekse publish eger dinleyecek belliyse send olarak gönedrmek lazım 
            await _publishEndpoint.Publish(orderCreatedEvent);

            return Ok();

        }

    }
}
