namespace StoreOfCoinsApi.GraphQL;

public class CoinInput
{
    public string Country { get; set; } = null!;
    public decimal Year { get; set; }
    public string Currency { get; set; } = null!;
    public decimal Value { get; set; }
    public decimal Price { get; set; }
    public string? ConfirmedByUserId { get; set; }
}
