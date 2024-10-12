using Microsoft.Extensions.Options;
using MongoDB.Driver;
using StoreOfCoinsApi.Models;

namespace StoreOfCoinsApi.Services;

    public class CoinsService
    {
        private readonly IMongoCollection<Coin> _coinsCollection;

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
        public async Task<List<Coin>> GetAsync() =>
        await _coinsCollection.Find(_ => true).ToListAsync();

        public async Task<Coin?> GetAsync(string id) =>
            await _coinsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Coin newCoin) =>
            await _coinsCollection.InsertOneAsync(newCoin);

        public async Task UpdateAsync(string id, Coin updatedCoin) =>
            await _coinsCollection.ReplaceOneAsync(x => x.Id == id, updatedCoin);

        public async Task RemoveAsync(string id) =>
            await _coinsCollection.DeleteOneAsync(x => x.Id == id);
    }
