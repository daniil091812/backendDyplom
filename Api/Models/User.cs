using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WebApplication1.Models;

public class User
{
    [BsonId]
    public ObjectId Id { get; set; }
    public int InternalId { get; set; }
    public string? Login { get; set; }
    public string? Password { get; set; }
    public int Points{ get; set; }
    public int[]? TourIds { get; set; }
}

public class Creds{
    public required string Username{get;set;}
    public required string Password{get;set;}
}