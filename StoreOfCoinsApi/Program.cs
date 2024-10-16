using StoreOfCoinsApi.Models;
using StoreOfCoinsApi.Services; // �������� ���� using, ���� ������ ��������� � ������ �����

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.Configure<StoreOfCoinsDatabaseSettings>(
    builder.Configuration.GetSection("StoreOfCoinsDatabase"));

// ����������� CoinsService
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

//�������� ����� �� ��� ��, � � ������������