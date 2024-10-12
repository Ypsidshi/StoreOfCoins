using StoreOfCoinsApi.Models;
using StoreOfCoinsApi.Services; // Добавьте этот using, если сервис находится в другой папке

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.Configure<StoreOfCoinsDatabaseSettings>(
    builder.Configuration.GetSection("StoreOfCoinsDatabase"));

// Регистрация CoinsService
builder.Services.AddScoped<CoinsService>();

builder.Services.AddControllers()
    .AddJsonOptions(
        options => options.JsonSerializerOptions.PropertyNamingPolicy = null);
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

//Добавить тесты не над БД, а с контроллером