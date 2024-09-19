using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ResQueue.Models;

public class Message
{
    [BsonId] public ObjectId Id { get; set; }
    public ObjectId UserId { get; set; }
    public ObjectId QueueId { get; set; }
    public required BsonValue Body { get; set; }
    public RabbitMQMessageMeta? RabbitMQMeta { get; set; }
    public bool IsReviewed { get; set; } = false;
    public required long MessageOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}