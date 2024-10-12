
namespace StoreOfCoinsApi.Models;

public class StoreOfCoinsDatabaseSettings
{
    public string ConnectionString { get; set; } = null!;

    public string DatabaseName { get; set; } = null!;

    public string CoinsCollectionName { get; set; } = null!;
}