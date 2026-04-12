using TaskManager.Application.Tasks.Contracts;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Services;

public interface ITaskService
{
    Task<TaskResponse> CreateAsync(CreateTaskRequest request, CancellationToken ct);
    Task<TaskResponse?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<TaskResponse>> GetAllAsync(TaskItemStatus? status, CancellationToken ct);
    Task<IReadOnlyList<TaskResponse>> GetDeletedAsync(CancellationToken ct);
    Task UpdateAsync(Guid id, UpdateTaskRequest request, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
    Task RestoreAsync(Guid id, CancellationToken ct);
}