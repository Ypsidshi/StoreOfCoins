using StoreOfCoinsApi.Models;
using StoreOfCoinsApi.Services; // �������� ���� using, ���� ������ ��������� � ������ �����
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Instrumentation.Runtime;
using OpenTelemetry.Instrumentation.Process;

var builder = WebApplication.CreateBuilder(args);

// ��������� Redis Cache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379"; // ����� Redis-�������
    options.InstanceName = "StoreOfCoins_";   // ������� ��� ������
});

// ��������� ��������� ��������
builder.Services.Configure<StoreOfCoinsDatabaseSettings>(
    builder.Configuration.GetSection("StoreOfCoinsDatabase"));

builder.Services.AddScoped<CoinsService>();
builder.Services.AddControllers()
    .AddJsonOptions(
        options => options.JsonSerializerOptions.PropertyNamingPolicy = null);
builder.Services.AddSwaggerGen();

// OpenTelemetry: Traces + Metrics with Prometheus exporter
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(
        serviceName: builder.Environment.ApplicationName,
        serviceVersion: typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddMeter("StoreOfCoins.Metrics")
        .AddRuntimeInstrumentation()
        .AddProcessInstrumentation()
        .AddPrometheusExporter());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseAuthorization();
// Prometheus scrape endpoint (/metrics)
app.MapPrometheusScrapingEndpoint();
app.MapControllers();
app.Run();

//�������� ����� �� ��� ��, � � ������������