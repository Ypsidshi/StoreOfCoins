using StoreOfCoinsApi.Models;
using StoreOfCoinsApi.Services;
using Microsoft.AspNetCore.Mvc;


namespace StoreOfCoinsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoinsController : ControllerBase
    {
        private readonly CoinsService _coinsService;

        public CoinsController(CoinsService coinsService) =>
            _coinsService = coinsService;

        [HttpGet]
        public async Task<List<Coin>> Get() =>
            await _coinsService.GetAsync();

        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<Coin>> Get(string id)
        {
            var coin = await _coinsService.GetAsync(id);

            if (coin is null)
            {
                return NotFound();
            }

            return coin;
        }

        [HttpPost]
        public async Task<IActionResult> Post(Coin newCoin)
        {
            await _coinsService.CreateAsync(newCoin);

            return CreatedAtAction(nameof(Get), new { id = newCoin.Id }, newCoin);
        }

        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Update(string id, Coin updatedCoin)
        {
            var coin = await _coinsService.GetAsync(id);

            if (coin is null)
            {
                return NotFound();
            }

            updatedCoin.Id = coin.Id;

            await _coinsService.UpdateAsync(id, updatedCoin);

            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var coin = await _coinsService.GetAsync(id);

            if (coin is null)
            {
                return NotFound();
            }

            await _coinsService.RemoveAsync(id);

            return NoContent();
        }
    }
}

