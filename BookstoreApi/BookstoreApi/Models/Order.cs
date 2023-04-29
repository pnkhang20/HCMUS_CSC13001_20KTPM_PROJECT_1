using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BookstoreApi.Models;

public class Order
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;
    public ICollection<OrderItem> OrderItemsList { get; set; } = null!;

    // Order shipping address
    [BsonElement("ShippingAddress")]     
    public string ShippingAddress { get; set; } = null!;
    
    // Order customer name
    [BsonElement("CustomerName")]
    [BsonRequired]
    public string CustomerName { get; set; } = null!;

    // Order customer phone number
    [BsonElement("CustomerPhone")]
    [BsonRequired]
    public string CustomerPhone { get; set; } = null!;

    // Order Subtotal
    [BsonElement("TotalPrice")]
    [BsonRequired]
    public decimal TotalPrice { get; set; }

    // Order status
    [BsonElement("IsDone")]
    [BsonRequired]
     public bool OrderIsDone { get; set; } = false;

    // Order date
    [BsonElement("OrderedDate")]
    [BsonRequired]
    public DateTime OrderedDate { get; set; }

    public Order()
    {
        OrderIsDone = false;
    }
}