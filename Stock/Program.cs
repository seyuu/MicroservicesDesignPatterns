using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared;
using Stock.Consumers;
using Stock.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseInMemoryDatabase("StockDb");
});

builder.Services.AddMassTransit(x => {
    x.AddConsumer<OrderCreatedEventConsumer>();
    x.AddConsumer<PaymentFailedEventConsumer>();
    x.UsingRabbitMq((context, configure) =>
    {
        configure.Host(builder.Configuration.GetConnectionString("RabbitMQ"));

        configure.ReceiveEndpoint(RabbitMQSettings.StockOrderCreatedEventQueueName, e =>
        {
            e.ConfigureConsumer<OrderCreatedEventConsumer>(context);
        });

        configure.ReceiveEndpoint(RabbitMQSettings.StockPaymentFailedEventQueueName, e =>
        {
            e.ConfigureConsumer<PaymentFailedEventConsumer>(context);
        });
    });
});
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// seed data oluþturuyoruz. 
// scope oluþturduktan sonra context nesnesini iþlem bitince memoryden kaldýrsýn diye scope oluþturdum
// uygulama ayaga kalkýnca bir kere çalýþýp biticek 
using (var scope = app.Services.CreateScope())
{
   var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    context.Stocks.Add(new Stock.Models.Stock() { Id = 1, ProductId = 1, Count = 100 });
    context.Stocks.Add(new Stock.Models.Stock() { Id = 2, ProductId = 2, Count = 300 });
    context.SaveChanges();
}

app.Run();
