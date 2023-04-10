using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BookstoreApi.Models;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("Name")]
    [BsonRequired]
    public string UserName { get; set; } = null!;
    [BsonRequired]
    public string Password { get; set; } = null!;

    public string FullName { get; set; } = null!;   
}
