using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace StoreOfCoinsApi.Models
{
    public class Coin
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        [JsonPropertyName("Country")]
        [BsonElement("Country")] //установки соответствия имен в проекте и коллеции БД
        public string Country { get; set; } = null!;
        public decimal Year { get; set; }
        public string Currency { get; set; } = null!;
        public decimal Value { get; set; }
        public decimal Price { get; set; }
        // Идентификатор пользователя, подтвердившего добавление
        [BsonElement("ConfirmedByUserId")]
        public string? ConfirmedByUserId { get; set; }
        // Время подтверждения (строковое, по заданию)
        [BsonElement("ConfirmationTime")]
        public string? ConfirmationTime { get; set; }
        // Строка с информацией о подтверждении
        [BsonElement("ConfirmationInfo")]
        public string? ConfirmationInfo { get; set; }
    }
}
