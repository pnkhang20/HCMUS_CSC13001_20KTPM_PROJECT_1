using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BookstoreApi.Models;

public class Category
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("CategoryName")]
    [BsonRequired]
    public string CategoryName { get; set; } = null!;

}