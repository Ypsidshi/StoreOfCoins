using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using StoreOfCoinsApi.Models;
using StoreOfCoinsApi.Services;
using System.Text.Json;
using System.Threading.Tasks;

namespace StoreOfCoinsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoinsController : ControllerBase
    {
        private readonly CoinsService _coinsService;
        private readonly IDistributedCache _cache;

        public CoinsController(CoinsService coinsService, IDistributedCache cache)
        {
            _coinsService = coinsService;
            _cache = cache;
        }

        // Метод для получения всех монет (кэширование не нужно)
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var coins = await _coinsService.GetAsync();
            return Ok(new { message = "Данные загружены из базы данных.", data = coins });
        }

        // Метод для получения монеты по ID с кэшированием
        [HttpGet("{id:length(24)}")]
        public async Task<IActionResult> Get(string id)
        {
            string cacheKey = $"coin_{id}";
            Coin? coin;

            // Попытка получить данные из кэша
            var cachedCoin = await _cache.GetStringAsync(cacheKey);
            if (cachedCoin != null)
            {
                coin = JsonSerializer.Deserialize<Coin>(cachedCoin);
                return Ok(new { message = $"Монета с ID {id} загружена из кэша.", data = coin });
            }

            // Если данные отсутствуют в кэше, загружаем их из базы данных
            coin = await _coinsService.GetAsync(id);
            if (coin == null)
            {
                return NotFound(new { message = $"Монета с ID {id} не найдена." });
            }

            // Кэшируем данные
            var cacheOptions = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(DateTimeOffset.UtcNow.AddMinutes(2));

            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(coin), cacheOptions);

            return Ok(new { message = $"Монета с ID {id} загружена из базы данных.", data = coin });
        }

        // Метод для создания новой монеты (POST) с очисткой кэша
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Coin newCoin)
        {
            await _coinsService.CreateAsync(newCoin);

            // Очищаем кэш для списка монет, чтобы в следующий раз получить обновленные данные
            await _cache.RemoveAsync("coinsList");

            return CreatedAtAction(nameof(Get), new { id = newCoin.Id }, newCoin);
        }

        // Метод для обновления существующей монеты (PUT) с обновлением кэша
        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Put(string id, [FromBody] Coin updatedCoin)
        {
            var coin = await _coinsService.GetAsync(id);

            if (coin == null)
            {
                return NotFound(new { message = $"Монета с ID {id} не найдена." });
            }

            updatedCoin.Id = coin.Id; // Устанавливаем тот же ID
            await _coinsService.UpdateAsync(id, updatedCoin);

            // Обновляем кэш для данной монеты
            string cacheKey = $"coin_{id}";
            var cacheOptions = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(DateTimeOffset.UtcNow.AddMinutes(2));

            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(updatedCoin), cacheOptions);

            // Очищаем кэш для списка монет, чтобы в следующий раз получить обновленные данные
            await _cache.RemoveAsync("coinsList");

            return NoContent();
        }

        // Метод для удаления монеты (DELETE) с удалением из кэша
        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var coin = await _coinsService.GetAsync(id);

            if (coin == null)
            {
                return NotFound(new { message = $"Монета с ID {id} не найдена." });
            }

            await _coinsService.RemoveAsync(id);

            // Удаляем данные из кэша для удаленной монеты
            string cacheKey = $"coin_{id}";
            await _cache.RemoveAsync(cacheKey);

            // Очищаем кэш для списка монет
            await _cache.RemoveAsync("coinsList");

            return NoContent();
        }
    }
}
