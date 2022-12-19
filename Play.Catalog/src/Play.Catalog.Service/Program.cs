using Play.Catalog.Service.Entities;
using Play.Common.MongoDB;
using Play.Common.Settings;

var builder = WebApplication.CreateBuilder(args);

ServiceSettings serviceSettings = new();
serviceSettings = builder.Configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();

// Add services to the container.
builder.Services.AddControllers(options =>
{
    options.SuppressAsyncSuffixInActionNames = false;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMongo()
                .AddMongoRepository<Item>("items");

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();/*(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Play.Catalog");
    options.RoutePrefix = string.Empty;
});
*/

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
