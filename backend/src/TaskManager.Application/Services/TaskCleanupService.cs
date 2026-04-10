using TaskManager.Application.Abstractions;

namespace TaskManager.Application.Services;


public sealed class TaskCleanupService : ITaskCleanupService
{
    private readonly ITaskRepository _repository;

    public TaskCleanupService(ITaskRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<int> CleanupSoftDeletedTasksAsync(
        int retentionDays = 30,
        CancellationToken ct = default)
    {
        if (retentionDays < 1)
            throw new ArgumentException("Retention days must be at least 1.", nameof(retentionDays));

        ct.ThrowIfCancellationRequested();

        // Uma query só — filtra no banco, não em memória
        var deletable = await _repository.GetSoftDeletedOlderThanAsync(retentionDays, ct);

        if (deletable.Count == 0)
            return 0;

        foreach (var task in deletable)
            _repository.Delete(task);

        await _repository.SaveChangesAsync(ct);

        return deletable.Count;
    }
}