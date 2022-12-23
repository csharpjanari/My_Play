using Play.Common.MongoDB;
using Play.Inventory.Service.Clients;
using Play.Inventory.Service.Entities;
using Polly;
using Polly.Timeout;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMongo()
                .AddMongoRepository<InventoryItem>("inventoryitems");

Random jitterer = new Random();

builder.Services.AddHttpClient<CatalogClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:6005");
})
.AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>().WaitAndRetryAsync(
    5,
    retryAttemt => TimeSpan.FromSeconds(Math.Pow(2, retryAttemt))
                    + TimeSpan.FromMilliseconds(jitterer.Next(0, 1000)),
    onRetry: (outcome, timespan, retryAttempt) =>
    {
        var serviceProvider = services.BuildServiceProvider();
        serviceProvider.GetService<ILogger<CatalogClient>>()
            .LogWarning($"Delaying for {timespan.TotalSeconds} seconds, then making retry {retryAttempt}");
    }
))
.AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(1));

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
