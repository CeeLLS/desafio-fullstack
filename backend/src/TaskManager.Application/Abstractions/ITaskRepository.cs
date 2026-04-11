using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
 
namespace TaskManager.Application.Abstractions;
 
public interface ITaskRepository
{
    Task AddAsync(TaskItem item, CancellationToken ct);
    Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken ct);
 
    Task<TaskItem?> GetByIdIgnoringFiltersAsync(Guid id, CancellationToken ct);
 
    Task<IReadOnlyList<TaskItem>> GetAllAsync(TaskItemStatus? status, CancellationToken ct);
    Task<IReadOnlyList<TaskItem>> GetSoftDeletedOlderThanAsync(int retentionDays, CancellationToken ct);
 
    void Update(TaskItem item);
    void Delete(TaskItem item);
    Task<int> SaveChangesAsync(CancellationToken ct);
}
 