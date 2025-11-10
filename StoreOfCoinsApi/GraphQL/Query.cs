using HotChocolate;
using HotChocolate.Data;
using StoreOfCoinsApi.Models;
using StoreOfCoinsApi.Services;
using System.Linq;

namespace StoreOfCoinsApi.GraphQL;

public class Query
{
    [UseFiltering]
    [UseSorting]
    public async Task<IQueryable<Coin>> Coins([Service] CoinsService service)
    {
        var list = await service.GetAsync();
        return list.AsQueryable();
    }

    public Task<Coin?> CoinById(string id, [Service] CoinsService service)
        => service.GetAsync(id);
}
