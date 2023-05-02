using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BookstoreApi.Models;

public class Book
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("Title")]
    [BsonRequired]
    public string Title { get; set; } = null!;

    [BsonElement("Author")]
    [BsonRequired]
    public string Author { get; set; } = null!;

    [BsonElement("Price")]
    [BsonRequired]
    public decimal Price { get; set; }

    [BsonElement("Quantity")]
    [BsonRequired]
    public int Quantity { get; set; }

    [BsonElement("Cover")]
    [BsonRequired]
    public string Cover { get; set; }

    [BsonElement("Category")]
    [BsonRequired]
    public Category Category { get; set; } = null!;

    [BsonElement("TotalSold")]
    [BsonRequired]
    public int TotalSold { get; set; } = 0;

}

