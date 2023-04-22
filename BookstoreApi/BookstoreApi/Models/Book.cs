﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Swashbuckle.AspNetCore.Annotations;

namespace BookstoreApi.Models;

public class Book
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [SwaggerSchema(ReadOnly = true)]
    public string Id { get; set; } = null!;

    [BsonElement("Name")]
    [BsonRequired]
    public string BookName { get; set; } = null!;
    public string Author { get; set; } = null!;

    public decimal Price { get; set; }

    public int Quantity { get; set; }

    public string Cover { get; set; }

    public ICollection<Category> Categories { get; set; } 

    

}