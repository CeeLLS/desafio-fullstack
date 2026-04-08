using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Abstractions;

public interface ITaskRepository
{
    Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<TaskItem>> GetAllAsync(TaskItemStatus? status, CancellationToken ct);
    Task AddAsync(TaskItem item, CancellationToken ct);
    void Update(TaskItem item);
    void Delete(TaskItem item);
    Task SaveChangesAsync(CancellationToken ct);
}