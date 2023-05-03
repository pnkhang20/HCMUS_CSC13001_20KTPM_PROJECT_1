using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BookstoreApi.Models;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;
    [BsonElement("Username")]
    [BsonRequired]
    public string UserName { get; set; } = null!;

    [BsonElement("Password")]
    [BsonRequired]
    public string Password { get; set; } = null!;

    [BsonElement("FullName")]
    public string FullName { get; set; } = null!;

    public User()
    {
        Id = ObjectId.GenerateNewId().ToString();
    }
}
