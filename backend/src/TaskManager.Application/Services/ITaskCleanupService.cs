namespace TaskManager.Application.Services;


public interface ITaskCleanupService
{
    Task<int> CleanupSoftDeletedTasksAsync(int retentionDays = 30, CancellationToken ct = default);
}
