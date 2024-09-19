using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using ResQueue.Mappers;
using ResQueue.Models;

namespace ResQueue.Features.Messages.SyncMessages;

public record SyncMessagesFeatureRequest(ClaimsPrincipal ClaimsPrincipal, string QueueId);

public record SyncMessagesFeatureResponse();

public class SyncMessagesFeature(
    UserManager<User> userManager,
    IMongoClient mongoClient,
    IMongoCollection<Queue> queuesCollection,
    IMongoCollection<Models.Broker> brokersCollection,
    IMongoCollection<Message> messagesCollection
) : ISyncMessagesFeature
{
    public async Task<OperationResult<SyncMessagesFeatureResponse>> ExecuteAsync(SyncMessagesFeatureRequest request)
    {
        var user = await userManager.GetUserAsync(request.ClaimsPrincipal);
        if (user == null)
        {
            return OperationResult<SyncMessagesFeatureResponse>.Failure(new ProblemDetails
            {
                Title = "Unauthorized Access",
                Detail = "The user could not be found or is not logged in.",
                Status = StatusCodes.Status401Unauthorized
            });
        }

        var queueFilter = Builders<Queue>.Filter.Eq(b => b.Id, ObjectId.Parse(request.QueueId));

        var queue = await queuesCollection.Find(queueFilter).SingleAsync();

        var brokerFilter = Builders<Models.Broker>.Filter.And(
            Builders<Models.Broker>.Filter.Eq(b => b.Id, queue.BrokerId),
            Builders<Models.Broker>.Filter.ElemMatch(b => b.AccessList, a => a.UserId == user.Id)
        );
        var broker = await brokersCollection.Find(brokerFilter).SingleAsync();

        var factory = RabbitmqConnectionFactory.CreateAmqpFactory(broker);
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();
        while (channel.BasicGet(queue.RawData.GetValue("name").AsString, false) is { } res)
        {
            var queueWithNewSequence = await queuesCollection.FindOneAndUpdateAsync(queueFilter,
                Builders<Queue>.Update.Inc(x => x.NextMessageOrder, 1));

            var message =
                RabbitMQMessageMapper.ToDocument(queue.Id, user.Id, queueWithNewSequence.NextMessageOrder, res);

            using var session = await mongoClient.StartSessionAsync();
            session.StartTransaction();

            await messagesCollection.InsertOneAsync(session, message);

            await queuesCollection.UpdateOneAsync(session, x => x.Id == queue.Id,
                Builders<Queue>.Update.Inc(x => x.RawData["messages"], -1));

            await queuesCollection.UpdateOneAsync(session, x => x.Id == queue.Id,
                Builders<Queue>.Update.Max(x => x.RawData["messages"], 0));

            await session.CommitTransactionAsync();

            // channel.BasicAck(res.DeliveryTag, false);
        }

        return OperationResult<SyncMessagesFeatureResponse>.Success(new SyncMessagesFeatureResponse());
    }
}