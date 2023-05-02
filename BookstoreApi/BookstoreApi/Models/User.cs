using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BookstoreApi.Models;

public class User
{
    [BsonId]
    [BsonElement("Id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("Username")]
    [BsonRequired]
    public string UserName { get; set; } = null!;

    [BsonElement("Password")]
    [BsonRequired]
    public string Password { get; set; } = null!;

    [BsonElement("FullName")]
    public string FullName { get; set; } = null!;
}
