using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace BookstoreApi.Models
{
    public class Book
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("name")]
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [BsonElement("author")]
        [JsonPropertyName("author")]
        public string Author { get; set; }

        [BsonElement("price")]
        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [BsonElement("quantity")]
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [BsonElement("cover")]
        [JsonPropertyName("cover")]
        public string Cover { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("categoryId")]
        [JsonPropertyName("categoryId")]
        public string CategoryId { get; set; }
    }
}
