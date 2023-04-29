using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace BookstoreApi.Models;

public class OrderItem
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("Book")]
    [BsonRequired]
    public Book Book { get; set; }

    [BsonElement("OrderItemQuantity")]
    [BsonRequired]
    public int Quantity { get; set; }

}
