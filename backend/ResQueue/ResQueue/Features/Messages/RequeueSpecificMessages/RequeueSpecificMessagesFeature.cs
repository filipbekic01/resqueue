using Dapper;
using Npgsql;
using ResQueue.Dtos;

namespace ResQueue.Features.Messages.MoveMessage;

public record RequeueSpecificMessagesRequest(
    RequeueSpecificMessagesDto Dto
);

public record RequeueSpecificMessagesResponse(
    int SucceededCount
);

public class RequeueSpecificMessagesFeature : IRequeueSpecificMessagesFeature
{
    public async Task<OperationResult<RequeueSpecificMessagesResponse>> ExecuteAsync(
        RequeueSpecificMessagesRequest request)
    {
        await using var connection =
            new NpgsqlConnection("host=localhost;port=5432;database=sandbox1;username=postgres;password=postgres;");

        await connection.OpenAsync();

        if (request.Dto.Transactional)
        {
            await using var transaction = await connection.BeginTransactionAsync();

            foreach (var messageDeliveryId in request.Dto.MessageDeliveryIds)
            {
                await CallRoutine(request, messageDeliveryId, connection);
            }

            await transaction.CommitAsync();

            return OperationResult<RequeueSpecificMessagesResponse>.Success(
                new RequeueSpecificMessagesResponse(request.Dto.MessageDeliveryIds.Length));
        }

        var succeededCount = 0;
        foreach (var messageDeliveryIds in request.Dto.MessageDeliveryIds)
        {
            if (await CallRoutine(request, messageDeliveryIds, connection) > 0)
            {
                succeededCount++;
            }
        }

        return OperationResult<RequeueSpecificMessagesResponse>.Success(
            new RequeueSpecificMessagesResponse(succeededCount));
    }

    private static async Task<int?> CallRoutine(RequeueSpecificMessagesRequest request, long deliveryMessageId,
        NpgsqlConnection connection)
    {
        var parameters = new DynamicParameters();
        parameters.Add("message_delivery_id", deliveryMessageId);
        parameters.Add("target_queue_type", request.Dto.TargetQueueType);
        parameters.Add("delay", request.Dto.Delay);
        parameters.Add("redelivery_count", request.Dto.RedeliveryCount);

        return await connection.QuerySingleAsync<int?>(
            $"SELECT transport.requeue_message(@message_delivery_id, @queue_type, @delay::interval, @redelivery_count)",
            parameters);
    }
}