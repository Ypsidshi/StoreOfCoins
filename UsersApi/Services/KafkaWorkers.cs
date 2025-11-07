using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using MongoDB.Driver;
using UsersApi.Models;

namespace UsersApi.Services;

public record ConfirmRequestMessage(string ObjectId, string UserId);
public record ConfirmResponseMessage(string ObjectId, string ConfirmationTime);

public class UsersConfirmConsumerProducer : BackgroundService
{
    private readonly ILogger<UsersConfirmConsumerProducer> _logger;
    private readonly IConsumer<string, string> _consumer;
    private readonly IProducer<string, string> _producer;
    private readonly IMongoCollection<User> _users;
    private readonly string _requestsTopic = "objects.confirm.requests";
    private readonly string _responsesTopic = "objects.confirm.responses";

    public UsersConfirmConsumerProducer(IConfiguration configuration, ILogger<UsersConfirmConsumerProducer> logger)
    {
        _logger = logger;

        var kafkaBootstrap = configuration.GetValue<string>("Kafka:BootstrapServers") ?? "kafka:9092";
        var mongoConnection = configuration.GetValue<string>("MongoDB:ConnectionString") ?? "mongodb://mongodb:27017";

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = kafkaBootstrap,
            GroupId = "storeofcoins-users-consumer",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            AllowAutoCreateTopics = true
        };
        _consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();

        var producerConfig = new ProducerConfig { BootstrapServers = kafkaBootstrap, Acks = Acks.All };
        _producer = new ProducerBuilder<string, string>(producerConfig).Build();

        var client = new MongoClient(mongoConnection);
        var db = client.GetDatabase("UsersDb");
        _users = db.GetCollection<User>("Users");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Даём время Kafka на запуск
        await Task.Delay(5000, stoppingToken);
        
        try
        {
            _consumer.Subscribe(_requestsTopic);
            _logger.LogInformation("Kafka consumer subscribed to topic: {Topic}", _requestsTopic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to subscribe to Kafka topic: {Topic}", _requestsTopic);
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var cr = _consumer.Consume(TimeSpan.FromSeconds(1));
                if (cr == null) continue;
                
                var msg = JsonSerializer.Deserialize<ConfirmRequestMessage>(cr.Message.Value);
                if (msg == null) continue;

                var update = Builders<User>.Update.Inc(u => u.RegisteredObjects, 1);
                var result = await _users.UpdateOneAsync(u => u.Id == msg.UserId, update, cancellationToken: stoppingToken);
                
                if (result.MatchedCount > 0)
                {
                    _logger.LogInformation("Incremented RegisteredObjects for user {UserId}", msg.UserId);
                }
                else
                {
                    _logger.LogWarning("User {UserId} not found, skipping increment", msg.UserId);
                }

                var response = new ConfirmResponseMessage(msg.ObjectId, DateTime.UtcNow.ToString("O"));
                var payload = JsonSerializer.Serialize(response);
                await _producer.ProduceAsync(_responsesTopic, new Message<string, string> { Key = msg.ObjectId, Value = payload }, stoppingToken);
                _logger.LogInformation("Sent confirmation response for object {ObjectId}", msg.ObjectId);
            }
            catch (OperationCanceledException) { break; }
            catch (ConsumeException ex)
            {
                _logger.LogWarning(ex, "Kafka consume error: {Error}", ex.Error.Reason);
                await Task.Delay(2000, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UsersConfirmConsumerProducer error");
                await Task.Delay(2000, stoppingToken);
            }
        }
    }
}


