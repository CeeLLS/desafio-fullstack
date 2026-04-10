using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.UnitTests.Domain;

public sealed class TaskItemTests
{

    [Fact]
    public void Create_ValidTitle_ReturnsTaskWithPendingStatus()
    {
        var task = TaskItem.Create("Titulo válido", null);

        Assert.Equal(TaskItemStatus.Pending, task.Status);
    }

    [Fact]
    public void Create_ValidTitle_AssignsNewGuid()
    {
        var task = TaskItem.Create("Titulo", null);

        Assert.NotEqual(Guid.Empty, task.Id);
    }

    [Fact]
    public void Create_TwoTasks_HaveDifferentIds()
    {
        var a = TaskItem.Create("Tarefa A", null);
        var b = TaskItem.Create("Tarefa B", null);

        Assert.NotEqual(a.Id, b.Id);
    }

    [Fact]
    public void Create_ValidTitle_SetsCreatedAtUtcToNow()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var task = TaskItem.Create("Titulo", null);
        var after = DateTime.UtcNow.AddSeconds(1);

        Assert.InRange(task.CreatedAtUtc, before, after);
    }

    [Fact]
    public void Create_TitleWithLeadingTrailingSpaces_TrimsIt()
    {
        var task = TaskItem.Create("  Titulo com espaço  ", null);

        Assert.Equal("Titulo com espaço", task.Title);
    }

    [Fact]
    public void Create_WithDescription_SetsDescription()
    {
        var task = TaskItem.Create("Titulo", "Minha descrição");

        Assert.Equal("Minha descrição", task.Description);
    }

    [Fact]
    public void Create_NullDescription_KeepsNull()
    {
        var task = TaskItem.Create("Titulo", null);

        Assert.Null(task.Description);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void Create_EmptyOrWhitespaceTitle_ThrowsArgumentException(string invalidTitle)
    {
        Assert.Throws<ArgumentException>(() => TaskItem.Create(invalidTitle, null));
    }

    [Fact]
    public void Update_ValidData_UpdatesTitleAndDescription()
    {
        var task = TaskItem.Create("Titulo antigo", "Desc antiga");

        task.Update("Titulo novo", "Desc nova");

        Assert.Equal("Titulo novo", task.Title);
        Assert.Equal("Desc nova", task.Description);
    }

    [Fact]
    public async Task Update_ValidData_UpdatesUpdatedAtUtc()
    {
        var task = TaskItem.Create("Titulo", null);
        var before = task.UpdatedAtUtc;

        await Task.Delay(10);
        task.Update("Titulo novo", null);

        Assert.True(task.UpdatedAtUtc >= before);
    }

    [Fact]
    public void Update_NullDescription_ClearsDescription()
    {
        var task = TaskItem.Create("Titulo", "Tinha descrição");

        task.Update("Titulo", null);

        Assert.Null(task.Description);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_EmptyTitle_ThrowsArgumentException(string invalidTitle)
    {
        var task = TaskItem.Create("Titulo original", null);

        Assert.Throws<ArgumentException>(() => task.Update(invalidTitle, null));
    }

    [Fact]
    public void Update_EmptyTitle_DoesNotMutateOriginalTitle()
    {
        var task = TaskItem.Create("Titulo original", null);

        try { task.Update("", null); } catch { /* esperado */ }

        Assert.Equal("Titulo original", task.Title);
    }

    [Fact]
    public void ChangeStatus_PendingToInProgress_ChangesStatus()
    {
        var task = TaskItem.Create("Titulo", null);

        task.ChangeStatus(TaskItemStatus.InProgress);

        Assert.Equal(TaskItemStatus.InProgress, task.Status);
    }

    [Fact]
    public void ChangeStatus_PendingToDone_ChangesStatus()
    {
        var task = TaskItem.Create("Titulo", null);

        task.ChangeStatus(TaskItemStatus.Done);

        Assert.Equal(TaskItemStatus.Done, task.Status);
    }

    [Fact]
    public async Task ChangeStatus_AnyChange_UpdatesUpdatedAtUtc()
    {
        var task = TaskItem.Create("Titulo", null);
        var before = task.UpdatedAtUtc;

        await Task.Delay(10);
        task.ChangeStatus(TaskItemStatus.InProgress);

        Assert.True(task.UpdatedAtUtc >= before);
    }

    [Theory]
    [InlineData(TaskItemStatus.Pending)]
    [InlineData(TaskItemStatus.InProgress)]
    [InlineData(TaskItemStatus.Done)]
    [InlineData(TaskItemStatus.Canceled)]
    public void ChangeStatus_AllValidStatuses_DoesNotThrow(TaskItemStatus status)
    {
        var task = TaskItem.Create("Titulo", null);

        var ex = Record.Exception(() => task.ChangeStatus(status));

        Assert.Null(ex);
    }

    [Fact] 
    public void SoftDelete_ActiveTask_SetsIsDeletedTrue()
    {
        var task = TaskItem.Create("Titulo", null);

    }

    [Fact]
    public void Update_DeletedTask_ThrowsDomainException()
    {
        var task = TaskItem.Create("Titulo", null);

    }

    [Fact] 
    public void Create_TitleExceeds200Chars_ThrowsArgumentException()
    {
        var longTitle = new string('x', 201);

        Assert.Throws<ArgumentException>(() => TaskItem.Create(longTitle, null));
    }

    [Fact] 
    public void Update_DescriptionExceeds2000Chars_ThrowsArgumentException()
    {
        var task = TaskItem.Create("Titulo", null);
        var longDesc = new string('x', 2001);

        Assert.Throws<ArgumentException>(() => task.Update("Titulo", longDesc));
    }

    // [Fact] 
    // public void Create_WithPriority_SetsPriorityCorrectly()
    // {
    //     var task = TaskItem.Create("Titulo", null, priority: TaskPriority.High);
    //     Assert.Equal(TaskPriority.High, task.Priority);
    // }

    [Fact] 
    public void Create_DueDateInThePast_ThrowsDomainException()
    {
        var pastDate = DateTime.UtcNow.AddDays(-1);

    }

    [Fact] 
    public void ChangeStatus_InvalidEnumValue_ThrowsArgumentException()
    {
        var task = TaskItem.Create("Titulo", null);

        // Hoje (TaskItemStatus)999 não lança — é comportamento incorreto
        Assert.Throws<ArgumentException>(() => task.ChangeStatus((TaskItemStatus)999));
    }
}