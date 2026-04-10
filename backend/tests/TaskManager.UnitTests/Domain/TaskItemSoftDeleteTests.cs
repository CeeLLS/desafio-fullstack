using TaskManager.Domain.Entities;
using Xunit;

namespace TaskManager.UnitTests.Domain;

public class TaskItemSoftDeleteTests
{
    [Fact]
    public void SoftDelete_UpdatesIsDeletedAndDeletedAtUtc()
    {
        var task = TaskItem.Create("Tarefa", null);
        var beforeDelete = DateTime.UtcNow;

        task.SoftDelete();
        var afterDelete = DateTime.UtcNow;

        Assert.True(task.IsDeleted);
        Assert.NotNull(task.DeletedAtUtc);
        Assert.True(task.DeletedAtUtc >= beforeDelete && task.DeletedAtUtc <= afterDelete);
    }

    [Fact]
    public void Restore_ClearsIsDeletedAndDeletedAtUtc()
    {
        var task = TaskItem.Create("Tarefa", null);
        task.SoftDelete();

        task.Restore();

        Assert.False(task.IsDeleted);
        Assert.Null(task.DeletedAtUtc);
    }

    [Fact]
    public void CanBePermanentlyDeleted_NotDeleted_ReturnsFalse()
    {
        var task = TaskItem.Create("Tarefa", null);

        var result = task.CanBePermanentlyDeleted(retentionDays: 30);

        Assert.False(result);
    }

    [Fact]
    public void CanBePermanentlyDeleted_DeletedButNoDeletedAtUtc_ReturnsFalse()
    {
        var task = TaskItem.Create("Tarefa", null);
        
        Assert.False(task.CanBePermanentlyDeleted(retentionDays: 30));
    }

    [Fact]
    public void CanBePermanentlyDeleted_RecentlyDeleted_ReturnsFalse()
    {
        var task = TaskItem.Create("Tarefa", null);
        task.SoftDelete();

        var result = task.CanBePermanentlyDeleted(retentionDays: 30);

        Assert.False(result);
    }

    [Fact]
    public void CanBePermanentlyDeleted_MinimalRetentionDays_ReturnsFalse()
    {
        var task = TaskItem.Create("Tarefa", null);
        task.SoftDelete();

        var result = task.CanBePermanentlyDeleted(retentionDays: 1);

        Assert.False(result);
    }

    [Fact]
    public void CanBePermanentlyDeleted_ZeroRetentionDays_ReturnsTrueForDeletedItems()
    {
        var task = TaskItem.Create("Tarefa", null);
        task.SoftDelete();

        var result = task.CanBePermanentlyDeleted(retentionDays: 0);

        Assert.True(result);
    }

    [Fact]
    public void SoftDelete_UpdatesUpdatedAtUtc()
    {
        var task = TaskItem.Create("Tarefa", null);
        var createdAt = task.UpdatedAtUtc;
        System.Threading.Thread.Sleep(100); // Garante diferença de tempo

        task.SoftDelete();

        Assert.True(task.UpdatedAtUtc > createdAt);
    }

    [Fact]
    public void Restore_UpdatesUpdatedAtUtc()
    {
        var task = TaskItem.Create("Tarefa", null);
        task.SoftDelete();
        var deletedAt = task.UpdatedAtUtc;
        System.Threading.Thread.Sleep(100);

        task.Restore();

        Assert.True(task.UpdatedAtUtc > deletedAt);
    }

    [Fact]
    public void MultipleRestores_WorkCorrectly()
    {
        var task = TaskItem.Create("Tarefa", null);

        task.SoftDelete();
        Assert.True(task.IsDeleted);

        task.Restore();
        Assert.False(task.IsDeleted);

        task.SoftDelete();
        Assert.True(task.IsDeleted);

        task.Restore();
        Assert.False(task.IsDeleted);
    }
}
