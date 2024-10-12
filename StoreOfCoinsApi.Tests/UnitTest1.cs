using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

    public CoinsControllerIntegrationTests()
    {
        // Настройка подключения к тестовой базе MongoDB
        var settings = new StoreOfCoinsDatabaseSettings
        {
            ConnectionString = "mongodb://localhost:27017",
            DatabaseName = "StoreOfCoins",
            CoinsCollectionName = "Coins"
        };

        var mongoClient = new MongoClient(settings.ConnectionString);
        _mongoDatabase = mongoClient.GetDatabase(settings.DatabaseName);
        _coinsService = new CoinsService(Options.Create(settings));
        _controller = new CoinsController(_coinsService);
    }

    // Генерация случайных данных для монет
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

    // Интеграционный тест: добавление 100 монет
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

        var result = await _controller.Get();

        // Assert
        Assert.Equal(100, result.Count);
    }

    // Интеграционный тест: добавление 100,000 монет
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

        var result = await _controller.Get();

        // Assert
        Assert.Equal(100000, result.Count);
    }

    // Интеграционный тест: удаление всех монет
    [Fact]
    public async Task DeleteAllCoins_IntegrationTest()
    {
        // Arrange
        var result = await _controller.Get();

        // Act
        foreach (var coin in result)
        {
            await _controller.Delete(coin.Id);
        }

        var finalResult = await _controller.Get();

        // Assert
        Assert.Empty(finalResult);
    }

    // Очистка коллекции после тестов
    public void Dispose()
    {
        _mongoDatabase.DropCollection("TestCoins");
    }
}
