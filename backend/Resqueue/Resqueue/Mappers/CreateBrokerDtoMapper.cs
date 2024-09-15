using MongoDB.Bson;
using Resqueue.Constants;
using Resqueue.Dtos;
using Resqueue.Enums;
using Resqueue.Models;

namespace Resqueue.Mappers;

public static class CreateBrokerDtoMapper
{
    public static Broker ToBroker(ObjectId userId, CreateBrokerDto dto)
    {
        var dateTime = DateTime.UtcNow;

        return new Broker
        {
            UserId = userId,
            AccessList =
            [
                new BrokerAccess
                {
                    UserId = userId,
                    AccessLevel = AccessLevel.Owner
                }
            ],
            System = BrokerSystems.RABBIT_MQ,
            Name = dto.Name,
            RabbitMQConnection = dto.RabbitMQConnection is { } rabbitMqConnection
                ? new()
                {
                    ManagementPort = rabbitMqConnection.ManagementPort,
                    ManagementTls = rabbitMqConnection.ManagementTls,
                    AmqpPort = rabbitMqConnection.AmqpPort,
                    AmqpTls = rabbitMqConnection.AmqpTls,
                    Host = rabbitMqConnection.Host,
                    Username = rabbitMqConnection.Username,
                    Password = rabbitMqConnection.Password,
                    VHost = rabbitMqConnection.VHost,
                }
                : null,
            CreatedAt = dateTime,
            UpdatedAt = dateTime
        };
    }
}