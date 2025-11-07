using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace UsersApi.Models;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public int RegisteredObjects { get; set; } = 0;
}


