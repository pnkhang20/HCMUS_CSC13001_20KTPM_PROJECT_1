using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BookstoreApi.Models;

public class Order
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("Name")]
    public string BookName { get; set; } = null!;

    public string Author { get; set; } = null!;

    public decimal Price { get; set; }

    public int Quantity { get; set; }

    public ICollection<Category> Categories { get; set; } = null!;

}