using HotChocolate;
using StoreOfCoinsApi.Models;
using StoreOfCoinsApi.Services;

namespace StoreOfCoinsApi.GraphQL;

public class Mutation
{
    public async Task<Coin> CreateCoin(CoinInput input, [Service] CoinsService service, [Service] IObjectsProducer producer)
    {
        var newCoin = new Coin
        {
            Country = input.Country,
            Year = input.Year,
            Currency = input.Currency,
            Value = input.Value,
            Price = input.Price,
            ConfirmedByUserId = input.ConfirmedByUserId
        };

        await service.CreateAsync(newCoin);

        if (!string.IsNullOrWhiteSpace(newCoin.ConfirmedByUserId) && !string.IsNullOrWhiteSpace(newCoin.Id))
        {
            await producer.SendConfirmRequestAsync(new ConfirmRequestMessage(newCoin.Id, newCoin.ConfirmedByUserId));
        }

        return newCoin;
    }

    public async Task<Coin?> UpdateCoin(string id, CoinInput input, [Service] CoinsService service)
    {
        var existing = await service.GetAsync(id);
        if (existing is null) return null;

        var updated = new Coin
        {
            Id = id,
            Country = input.Country,
            Year = input.Year,
            Currency = input.Currency,
            Value = input.Value,
            Price = input.Price,
            ConfirmedByUserId = input.ConfirmedByUserId,
            // Preserve confirmation fields if already set or updated later by Kafka
            ConfirmationTime = existing.ConfirmationTime,
            ConfirmationInfo = existing.ConfirmationInfo
        };

        await service.UpdateAsync(id, updated);
        return updated;
    }

    public async Task<bool> DeleteCoin(string id, [Service] CoinsService service)
    {
        var existing = await service.GetAsync(id);
        if (existing is null) return false;

        await service.RemoveAsync(id);
        return true;
    }
}
