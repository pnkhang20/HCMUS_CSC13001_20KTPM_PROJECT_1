using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;
namespace BookstoreApi.Models;

public class OrderItem
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("orderQuantity")]
    [BsonRequired]
    public string Quantity { get; set; }

    [BsonElement("Book")]
    [BsonRequired]
    public Book Book { get; set; }

}
