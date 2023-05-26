using MassTransit;
using Payment.Consumers;
using Shared;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//builder.Services.AddDbContext<AppDbContext>(options =>
//{
//    options.UseInMemoryDatabase("StockDb");
//});

builder.Services.AddMassTransit(x => {
    x.AddConsumer<StockReservedEventConsumer>();
    x.UsingRabbitMq((context, configure) =>
    {
        configure.Host(builder.Configuration.GetConnectionString("RabbitMQ"));

        configure.ReceiveEndpoint(RabbitMQSettings.StockReservedEventQueueName, e =>
        {
            e.ConfigureConsumer<StockReservedEventConsumer>(context);
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

app.Run();
