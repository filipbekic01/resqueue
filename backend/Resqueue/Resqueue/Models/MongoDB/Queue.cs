using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Resqueue.Models.MongoDB
{
    public class Queue
    {
        [BsonId] public ObjectId Id { get; set; }
        public ObjectId BrokerId { get; set; }
        public string Name { get; set; }
    }
}