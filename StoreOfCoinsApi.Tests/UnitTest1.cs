using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using StoreOfCoinsApi.Controllers;
using StoreOfCoinsApi.Models;
using StoreOfCoinsApi.Services;
using Xunit;

public class CoinsControllerIntegrationTests : IDisposable
{
    private readonly CoinsService _coinsService;
    private readonly CoinsController _controller;
    private readonly IMongoDatabase _mongoDatabase;
    private readonly IDistributedCache _cache;

    public CoinsControllerIntegrationTests()
    {
        // ��������� ����������� � �������� ���� MongoDB
        var settings = new StoreOfCoinsDatabaseSettings
        {
            ConnectionString = "mongodb://localhost:27017",
            DatabaseName = "StoreOfCoins",
            CoinsCollectionName = "Coins"
        };

        var mongoClient = new MongoClient(settings.ConnectionString);
        _mongoDatabase = mongoClient.GetDatabase(settings.DatabaseName);
        _coinsService = new CoinsService(Options.Create(settings));
        _cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        _controller = new CoinsController(_coinsService, _cache);
    }

    // ��������� ��������� ������ ��� �����
    private List<Coin> GenerateRandomCoins(int count)
    {
        var random = new Random();
        var coins = new List<Coin>();
        for (int i = 0; i < count; i++)
        {
            coins.Add(new Coin
            {
                Id = Guid.NewGuid().ToString(),
                Country = $"Country {i}",
                Year = random.Next(1800, 2024),
                Currency = $"Currency {i}",
                Value = random.Next(1, 1000) * 0.01m,
                Price = random.Next(10, 1000) * 0.01m
            });
        }
        return coins;
    }

    // �������������� ����: ���������� 100 �����
    [Fact]
    public async Task Add100Coins_IntegrationTest()
    {
        // Arrange
        var coins = GenerateRandomCoins(100);

        // Act
        foreach (var coin in coins)
        {
            await _controller.Post(coin);
        }

        var action = await _controller.Get();
        var ok = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(action);
        var payload = Assert.IsType<dynamic>(ok.Value);
        var result = (IEnumerable<Coin>)payload.data;

        // Assert
        Assert.Equal(100, System.Linq.Enumerable.Count(result));
    }

    // �������������� ����: ���������� 100,000 �����
    [Fact]
    public async Task Add100000Coins_IntegrationTest()
    {
        // Arrange
        var coins = GenerateRandomCoins(100000);

        // Act
        foreach (var coin in coins)
        {
            await _controller.Post(coin);
        }

        var action = await _controller.Get();
        var ok = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(action);
        var payload = Assert.IsType<dynamic>(ok.Value);
        var result = (IEnumerable<Coin>)payload.data;

        // Assert
        Assert.Equal(100000, System.Linq.Enumerable.Count(result));
    }

    // �������������� ����: �������� ���� �����
    [Fact]
    public async Task DeleteAllCoins_IntegrationTest()
    {
        // Arrange
        var action = await _controller.Get();
        var ok = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(action);
        var payload = Assert.IsType<dynamic>(ok.Value);
        var result = (IEnumerable<Coin>)payload.data;

        // Act
        foreach (var coin in result)
        {
            await _controller.Delete(coin.Id);
        }

        var finalAction = await _controller.Get();
        var finalOk = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(finalAction);
        var finalPayload = Assert.IsType<dynamic>(finalOk.Value);
        var finalResult = (IEnumerable<Coin>)finalPayload.data;

        // Assert
        Assert.Empty(finalResult);
    }

    // ������� ��������� ����� ������
    public void Dispose()
    {
        _mongoDatabase.DropCollection("TestCoins");
    }
}
