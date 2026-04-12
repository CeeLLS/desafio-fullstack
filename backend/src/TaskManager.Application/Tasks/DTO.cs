namespace TaskManager.Application.Tasks.Contracts;

public sealed record CreateTaskRequest(string Title, string? Description);

public sealed record UpdateTaskRequest(string Title, string? Description, int Status);

public sealed record TaskResponse(
    Guid Id,
    string Title,
    string? Description,
    int Status,
    bool IsDeleted,
    DateTime? DeletedAtUtc,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);