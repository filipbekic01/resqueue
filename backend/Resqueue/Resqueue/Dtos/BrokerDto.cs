namespace Resqueue.Dtos;

public record BrokerDto(
    string Id,
    string Name,
    int Port,
    string Url,
    string Framework,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? SyncedAt,
    DateTime? DeletedAt
);