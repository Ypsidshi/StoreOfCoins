using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace StoreOfCoinsApi.Models
{
    public class Coin
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("Name")]
        public string Country { get; set; } = null!;
        public decimal Year { get; set; }
        public string Currency { get; set; } = null!;
        public decimal Value { get; set; }
        public decimal Price { get; set; }

      
    }
}
