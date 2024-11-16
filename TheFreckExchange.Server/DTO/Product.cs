﻿using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace TheFreckExchange.Server.DTO
{
    public class Product
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public virtual string? Id { get; set; }
        [BsonElement("Name")]
        public required string Name { get; set; }
        [BsonElement("ProductId")]
        public required string ProductId { get; init; }
        public required string ProductDescription { get; set; }
        public List<string> AvailableAttributes { get; set; } = new List<string>();
        public required double Price { get; set; }
        public List<byte[]> ImageBytes { get; set; } = new List<byte[]>();
        public string? ProductOwnerId { get; set; }

        public Product()
        {
            ProductId = Guid.NewGuid().ToString();
        }
    }

    public class ProductDTO
    {
        public required string Name { get; set; }
        public required double Price { get; set; }
        public required string Description { get; set; }
        public LoginCredentials? Credentials { get; set; }
        public List<string> Attributes { get; set; } = new List<string>();
        public List<byte[]> ImageBytes { get; set; } = new List<byte[]>();
    }

    public class Item : Product
    {
        [BsonElement("SKU")]
        public required string SKU { get; set; } = Guid.NewGuid().ToString();
        public List<ItemAttribute> Attributes { get; set; } = new List<ItemAttribute>();
        public LoginCredentials? Credentials { get; set; }
        public string SellerId { get; set; } = String.Empty;
    }

    public class ItemDTO
    {
        public required string Name { get; set; } = String.Empty;
        public LoginCredentials? Credentials { get; set; }
        public List<ItemAttribute> Attributes { get; set; } = new List<ItemAttribute>();
        public string SellerId { get; set; } = String.Empty;
    }

    public class ItemAttribute
    {
        public required string Type { get; set; }
        public required string Value { get; set; }
    }
}