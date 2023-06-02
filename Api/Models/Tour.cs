using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WebApplication1.Models;

public class Tour
{
    [BsonId]
    public ObjectId Id { get; set; }
    public int InternalId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }

    public string? PicturePath { get; set; }
    public int Price { get; set; }
}