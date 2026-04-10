using TaskManager.Domain.Enums;

namespace TaskManager.Domain.Entities;

public class TaskItem
{
    private const int MaxTitleLength = 200;
    private const int MaxDescriptionLength = 2000;

    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public TaskItemStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public DateTime? DeletedAtUtc { get; private set; }
    
    public bool IsDeleted { get; private set; }

    private TaskItem() { }

    public static TaskItem Create(string title, string? description)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));

        if (title.Length > MaxTitleLength)
            throw new ArgumentException($"Title cannot exceed {MaxTitleLength} characters.", nameof(title));

        if (!string.IsNullOrEmpty(description) && description.Length > MaxDescriptionLength)
            throw new ArgumentException($"Description cannot exceed {MaxDescriptionLength} characters.", nameof(description));

        return new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = title.Trim(),
            Description = description?.Trim(),
            Status = TaskItemStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
            IsDeleted = false
        };
    }

    public void Update(string title, string? description)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));

        if (title.Length > MaxTitleLength)
            throw new ArgumentException($"Title cannot exceed {MaxTitleLength} characters.", nameof(title));

        if (!string.IsNullOrEmpty(description) && description.Length > MaxDescriptionLength)
            throw new ArgumentException($"Description cannot exceed {MaxDescriptionLength} characters.", nameof(description));

        Title = title.Trim();
        Description = description?.Trim();
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void ChangeStatus(TaskItemStatus status)
    {
        if (!Enum.IsDefined(typeof(TaskItemStatus), status))
            throw new ArgumentException($"Invalid status value: {status}.", nameof(status));

        Status = status;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Restore()
    {
        IsDeleted = false;
        DeletedAtUtc = null;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public bool CanBePermanentlyDeleted(int retentionDays = 30)
    {
        if (!IsDeleted || !DeletedAtUtc.HasValue)
            return false;

        var daysDeleted = (DateTime.UtcNow - DeletedAtUtc.Value).TotalDays;
        return daysDeleted >= retentionDays;
    }
}