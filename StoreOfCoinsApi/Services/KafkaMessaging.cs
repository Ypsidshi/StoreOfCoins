using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StoreOfCoinsApi.Models;
using System.Text.Json;

namespace StoreOfCoinsApi.Services;

public class KafkaOptions
{
    public string BootstrapServers { get; set; } = "localhost:9092";
    public TopicsOptions Topics { get; set; } = new();
    public GroupIdsOptions GroupIds { get; set; } = new();

    public class TopicsOptions
    {
        public string ObjectsConfirmRequests { get; set; } = "objects.confirm.requests";
        public string ObjectsConfirmResponses { get; set; } = "objects.confirm.responses";
    }

    public class GroupIdsOptions
    {
        public string ObjectsConsumer { get; set; } = "storeofcoins-objects-consumer";
        public string UsersConsumer { get; set; } = "storeofcoins-users-consumer";
    }
}

public record ConfirmRequestMessage(string ObjectId, string UserId);
public record ConfirmResponseMessage(string ObjectId, string ConfirmationTime);

public interface IObjectsProducer
{
    Task SendConfirmRequestAsync(ConfirmRequestMessage message, CancellationToken ct = default);
}

public class ObjectsProducer : IObjectsProducer
{
    private readonly IProducer<string, string> _producer;
    private readonly string _topic;
    private readonly ILogger<ObjectsProducer> _logger;

    public ObjectsProducer(IOptions<KafkaOptions> options, ILogger<ObjectsProducer> logger)
    {
        _logger = logger;
        var config = new ProducerConfig
        {
            BootstrapServers = options.Value.BootstrapServers,
            Acks = Acks.All
        };
        _producer = new ProducerBuilder<string, string>(config).Build();
        _topic = options.Value.Topics.ObjectsConfirmRequests;
    }

    public async Task SendConfirmRequestAsync(ConfirmRequestMessage message, CancellationToken ct = default)
    {
        var payload = JsonSerializer.Serialize(message);
        await _producer.ProduceAsync(_topic, new Message<string, string>
        {
            Key = message.ObjectId,
            Value = payload
        }, ct);
        _logger.LogInformation("Kafka: sent confirm request for object {ObjectId}", message.ObjectId);
    }
}

public class ObjectsConfirmResponseConsumer : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<ObjectsConfirmResponseConsumer> _logger;
    private readonly IConsumer<string, string> _consumer;
    private readonly string _topic;

    public ObjectsConfirmResponseConsumer(IOptions<KafkaOptions> options, IServiceProvider services, ILogger<ObjectsConfirmResponseConsumer> logger)
    {
        _services = services;
        _logger = logger;
        var config = new ConsumerConfig
        {
            BootstrapServers = options.Value.BootstrapServers,
            GroupId = options.Value.GroupIds.ObjectsConsumer,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true,
            AllowAutoCreateTopics = true
        };
        _consumer = new ConsumerBuilder<string, string>(config).Build();
        _topic = options.Value.Topics.ObjectsConfirmResponses;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Даём время Kafka на запуск
        await Task.Delay(5000, stoppingToken);
        
        try
        {
            _consumer.Subscribe(_topic);
            _logger.LogInformation("Kafka consumer subscribed to topic: {Topic}", _topic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to subscribe to Kafka topic: {Topic}", _topic);
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var cr = _consumer.Consume(TimeSpan.FromSeconds(1));
                if (cr == null) continue;
                
                var message = JsonSerializer.Deserialize<ConfirmResponseMessage>(cr.Message.Value);
                if (message == null) continue;

                using var scope = _services.CreateScope();
                var coins = scope.ServiceProvider.GetRequiredService<CoinsService>();
                var coin = await coins.GetAsync(message.ObjectId);
                if (coin != null)
                {
                    coin.ConfirmationTime = message.ConfirmationTime;
                    coin.ConfirmationInfo = $"Подтверждено {message.ConfirmationTime}";
                    await coins.UpdateAsync(message.ObjectId, coin);
                    _logger.LogInformation("Updated coin {ObjectId} with confirmation time", message.ObjectId);
                }
            }
            catch (OperationCanceledException) { break; }
            catch (ConsumeException ex)
            {
                _logger.LogWarning(ex, "Kafka consume error: {Error}", ex.Error.Reason);
                await Task.Delay(2000, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kafka consumer error");
                await Task.Delay(2000, stoppingToken);
            }
        }
    }
}



