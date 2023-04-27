using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BookstoreApi.Models;

public class Order
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;
    public ICollection<OrderItem> OrderItemsList { get; set; } = null!;
    public string ShippingAddress { get; set; } = null!;
    public string CustomerName { get; set; } = null!;    
    public string CustomerPhone { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime OrderedDate { get; set; }

}