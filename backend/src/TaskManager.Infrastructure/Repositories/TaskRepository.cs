using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Abstractions;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Infrastructure.Persistence;

namespace TaskManager.Infrastructure.Repositories;

public sealed class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _db;

    public TaskRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(TaskItem item, CancellationToken ct)
        => await _db.TaskItems.AddAsync(item, ct);

    public async Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken ct)
        => await _db.TaskItems.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<TaskItem>> GetAllAsync(TaskItemStatus? status, CancellationToken ct)
    {
        var query = _db.TaskItems.AsQueryable();

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        return await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<TaskItem>> GetSoftDeletedOlderThanAsync(
        int retentionDays,
        CancellationToken ct)
    {
        var cutoff = DateTime.UtcNow.AddDays(-retentionDays);

        return await _db.TaskItems
            .IgnoreQueryFilters()
            .Where(x => x.IsDeleted && x.DeletedAtUtc.HasValue && x.DeletedAtUtc.Value <= cutoff)
            .ToListAsync(ct);
    }

    public void Update(TaskItem item) => _db.TaskItems.Update(item);

    public void Delete(TaskItem item) => _db.TaskItems.Remove(item);

    public Task<int> SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}