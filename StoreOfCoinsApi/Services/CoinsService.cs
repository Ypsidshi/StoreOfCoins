using Microsoft.Extensions.Options;
using MongoDB.Driver;
using StoreOfCoinsApi.Models;
using System.Diagnostics.Metrics;

namespace StoreOfCoinsApi.Services;

    public class CoinsService
    {
        private readonly IMongoCollection<Coin> _coinsCollection;
        private static readonly Meter TelemetryMeter = new("StoreOfCoins.Metrics", "1.0.0");
        private static readonly Counter<long> CoinsReadCounter = TelemetryMeter.CreateCounter<long>("coins_read_total", unit: "items", description: "Total coins read operations");
        private static readonly Counter<long> CoinsWriteCounter = TelemetryMeter.CreateCounter<long>("coins_write_total", unit: "items", description: "Total coins write operations");
        private static readonly Histogram<double> MongoRequestDurationMs = TelemetryMeter.CreateHistogram<double>("mongo_request_duration_ms", unit: "ms", description: "MongoDB request duration");

        public CoinsService(
            IOptions<StoreOfCoinsDatabaseSettings> coinStoreDatabaseSettings)
        {
            var mongoClient = new MongoClient(
                coinStoreDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                coinStoreDatabaseSettings.Value.DatabaseName);

            _coinsCollection = mongoDatabase.GetCollection<Coin>(
                coinStoreDatabaseSettings.Value.CoinsCollectionName);
        }
        public async Task<List<Coin>> GetAsync()
        {
            var start = System.Diagnostics.Stopwatch.GetTimestamp();
            var result = await _coinsCollection.Find(_ => true).ToListAsync();
            CoinsReadCounter.Add(result.Count);
            var elapsedMs = (System.Diagnostics.Stopwatch.GetTimestamp() - start) * 1000.0 / System.Diagnostics.Stopwatch.Frequency;
            MongoRequestDurationMs.Record(elapsedMs, KeyValuePair.Create<string, object?>("operation", "find_all"));
            return result;
        }

        public async Task<Coin?> GetAsync(string id)
        {
            var start = System.Diagnostics.Stopwatch.GetTimestamp();
            var coin = await _coinsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
            CoinsReadCounter.Add(1);
            var elapsedMs = (System.Diagnostics.Stopwatch.GetTimestamp() - start) * 1000.0 / System.Diagnostics.Stopwatch.Frequency;
            MongoRequestDurationMs.Record(elapsedMs, KeyValuePair.Create<string, object?>("operation", "find_one"));
            return coin;
        }

        public async Task CreateAsync(Coin newCoin)
        {
            var start = System.Diagnostics.Stopwatch.GetTimestamp();
            await _coinsCollection.InsertOneAsync(newCoin);
            CoinsWriteCounter.Add(1, KeyValuePair.Create<string, object?>("method", "insert_one"));
            var elapsedMs = (System.Diagnostics.Stopwatch.GetTimestamp() - start) * 1000.0 / System.Diagnostics.Stopwatch.Frequency;
            MongoRequestDurationMs.Record(elapsedMs, KeyValuePair.Create<string, object?>("operation", "insert_one"));
        }

        public async Task UpdateAsync(string id, Coin updatedCoin)
        {
            var start = System.Diagnostics.Stopwatch.GetTimestamp();
            await _coinsCollection.ReplaceOneAsync(x => x.Id == id, updatedCoin);
            CoinsWriteCounter.Add(1, KeyValuePair.Create<string, object?>("method", "replace_one"));
            var elapsedMs = (System.Diagnostics.Stopwatch.GetTimestamp() - start) * 1000.0 / System.Diagnostics.Stopwatch.Frequency;
            MongoRequestDurationMs.Record(elapsedMs, KeyValuePair.Create<string, object?>("operation", "replace_one"));
        }

        public async Task RemoveAsync(string id)
        {
            var start = System.Diagnostics.Stopwatch.GetTimestamp();
            await _coinsCollection.DeleteOneAsync(x => x.Id == id);
            CoinsWriteCounter.Add(1, KeyValuePair.Create<string, object?>("method", "delete_one"));
            var elapsedMs = (System.Diagnostics.Stopwatch.GetTimestamp() - start) * 1000.0 / System.Diagnostics.Stopwatch.Frequency;
            MongoRequestDurationMs.Record(elapsedMs, KeyValuePair.Create<string, object?>("operation", "delete_one"));
        }
    }
