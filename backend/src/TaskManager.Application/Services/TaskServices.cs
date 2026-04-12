using TaskManager.Application.Abstractions;
using TaskManager.Application.Tasks.Contracts;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Services;

public sealed class TaskService : ITaskService
{
    private const int MaxTitleLength = 200;
    private readonly ITaskRepository _repository;

    public TaskService(ITaskRepository repository)
        => _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public async Task<TaskResponse> CreateAsync(CreateTaskRequest request, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (request.Title.Length > MaxTitleLength)
            throw new ArgumentException($"Title cannot exceed {MaxTitleLength} characters.");

        var item = TaskItem.Create(request.Title, request.Description);
        await _repository.AddAsync(item, ct);
        await _repository.SaveChangesAsync(ct);
        return Map(item);
    }

    public async Task<TaskResponse?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var item = await _repository.GetByIdAsync(id, ct);
        return item is null ? null : Map(item);
    }

    public async Task<IReadOnlyList<TaskResponse>> GetAllAsync(TaskItemStatus? status, CancellationToken ct)
    {
        var items = await _repository.GetAllAsync(status, ct);
        return items.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<TaskResponse>> GetDeletedAsync(CancellationToken ct)
    {
        var items = await _repository.GetAllDeletedAsync(ct);
        return items.Select(Map).ToList();
    }

    public async Task UpdateAsync(Guid id, UpdateTaskRequest request, CancellationToken ct)
    {
        var item = await _repository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Task not found.");

        if (!Enum.IsDefined(typeof(TaskItemStatus), request.Status))
            throw new ArgumentException($"Invalid status value: {request.Status}.");

        if (request.Title.Length > MaxTitleLength)
            throw new ArgumentException($"Title cannot exceed {MaxTitleLength} characters.");

        item.Update(request.Title, request.Description);
        item.ChangeStatus((TaskItemStatus)request.Status);
        _repository.Update(item);
        await _repository.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var item = await _repository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Task not found.");

        item.SoftDelete();
        _repository.Update(item);
        await _repository.SaveChangesAsync(ct);
    }

    public async Task RestoreAsync(Guid id, CancellationToken ct)
    {
        var item = await _repository.GetByIdIgnoringFiltersAsync(id, ct)
            ?? throw new KeyNotFoundException("Task not found.");

        if (!item.IsDeleted) return; // idempotente

        item.Restore();
        _repository.Update(item);
        await _repository.SaveChangesAsync(ct);
    }

    private static TaskResponse Map(TaskItem item) =>
        new(item.Id, item.Title, item.Description, (int)item.Status,
            item.IsDeleted, item.DeletedAtUtc, item.CreatedAtUtc, item.UpdatedAtUtc);
}