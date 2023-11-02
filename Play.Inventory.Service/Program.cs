using MassTransit;
using Play.Common;
using Play.Common.MassTransit;
using Play.Common.MongoDB;
using Play.Inventory.Service.Clients;
using Play.Inventory.Service.Entities;
using Polly;
using Polly.Timeout;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services
    .AddMongo()
    .AddMongoRepository<InventoryItem>("inventory_items")
    .AddMongoRepository<CatalogItem>("catalog_items")
    .AddMassTransitWithRabbitMQ();

AddCatalogClient(builder);

builder.Services.AddControllers(options =>
{
    options.SuppressAsyncSuffixInActionNames = false;
});
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

static void AddCatalogClient(WebApplicationBuilder builder)
{
    var jitterer = new Random();

    builder.Services.AddHttpClient<CatalogClient>(client =>
    {
        client.BaseAddress = new Uri("http://localhost:5001");
    })
    .AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>().WaitAndRetryAsync(
        5,
        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(jitterer.Next(0, 1000)),
        onRetry: (response, timespan, retryCount, context) =>
        {
            // TODO: Log using Ilogger
            var msg = $"==>> Retry {retryCount} for {context?.PolicyKey} at {context?.OperationKey}, due to: {response?.Result?.StatusCode}";
            Console.WriteLine(msg);
        }
    ))
    .AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>().CircuitBreakerAsync(
        3,
        TimeSpan.FromSeconds(15),
        onBreak: (outcome, timespan) =>
        {
            var msg = $"==>> Circuit breaker opened for {timespan.TotalSeconds} seconds due to: {outcome.Exception?.Message ?? outcome.Result.StatusCode.ToString()}";
            Console.WriteLine(msg);
        },
        onReset: () => Console.WriteLine("==>> Circuit breaker reset")
    ))
    .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(1)));
}
