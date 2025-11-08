using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using StoreOfCoinsApi.Models;
using StoreOfCoinsApi.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StoreOfCoinsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly CoinsService _coinsService;

        public TestController(CoinsService coinsService)
        {
            _coinsService = coinsService;
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
                    Id = ObjectId.GenerateNewId().ToString(),  // Генерация корректного ObjectId
                    Country = $"Country {i}",
                    Year = random.Next(1800, 2024),
                    Currency = $"Currency {i}",
                    Value = random.Next(1, 1000) * 0.01m,
                    Price = random.Next(10, 1000) * 0.01m
                });
            }
            return coins;
        }

        // Тест 1: Добавление 100 монет
        [HttpPost("add-100-coins")]
        public async Task<IActionResult> Add100Coins()
        {
            var coins = GenerateRandomCoins(100);

            foreach (var coin in coins)
            {
                await _coinsService.CreateAsync(coin);
            }

            return Ok(new { message = "100 монет успешно добавлены в базу данных." });
        }

        // Тест 2: Добавление 1000 монет
        [HttpPost("add-1000-coins")]
        public async Task<IActionResult> Add1000Coins()
        {
            var coins = GenerateRandomCoins(1000);

            foreach (var coin in coins)
            {
                await _coinsService.CreateAsync(coin);
            }

            return Ok(new { message = "100,000 монет успешно добавлены в базу данных." });
        }

        // Тест 3: Удаление всех монет
        [HttpDelete("delete-all-coins")]
        public async Task<IActionResult> DeleteAllCoins()
        {
            var coins = await _coinsService.GetAsync();

            foreach (var coin in coins)
            {
                await _coinsService.RemoveAsync(coin.Id);
            }

            return Ok(new { message = "Все монеты успешно удалены." });
        }
    }
}
