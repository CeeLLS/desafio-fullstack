using Moq;
using TaskManager.Application.Abstractions;
using TaskManager.Application.Services;
using TaskManager.Application.Tasks.Contracts;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.UnitTests.Services;

public sealed class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _repositoryMock;
    private readonly TaskService _sut; 

    public TaskServiceTests()
    {
        _repositoryMock = new Mock<ITaskRepository>(MockBehavior.Strict);
        _sut = new TaskService(_repositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsMappedResponse()
    {
        var request = new CreateTaskRequest("Comprar café", "Café coado, não solúvel");

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(1));

        var result = await _sut.CreateAsync(request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Comprar café", result.Title);
        Assert.Equal("Café coado, não solúvel", result.Description);
        Assert.Equal((int)TaskItemStatus.Pending, result.Status);
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_CallsAddAndSaveExactlyOnce()
    {
        var request = new CreateTaskRequest("Titulo qualquer", null);

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(1));

        await _sut.CreateAsync(request, CancellationToken.None);

        _repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _repositoryMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_NewTask_StatusIsPendingByDefault()
    {
        var request = new CreateTaskRequest("Tarefa nova", null);

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(1));

        var result = await _sut.CreateAsync(request, CancellationToken.None);

        Assert.Equal((int)TaskItemStatus.Pending, result.Status);
    }

    [Fact]
    public async Task CreateAsync_TitleWithWhitespace_TrimsTitle()
    {
        var request = new CreateTaskRequest("  Titulo com espaço  ", null);

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(1));

        var result = await _sut.CreateAsync(request, CancellationToken.None);

        Assert.Equal("Titulo com espaço", result.Title);
    }

    [Fact]
    public async Task CreateAsync_EmptyTitle_ThrowsArgumentException()
    {
        var request = new CreateTaskRequest("", null);

        await Assert.ThrowsAsync<ArgumentException>(
            () => _sut.CreateAsync(request, CancellationToken.None));

        _repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhitespaceOnlyTitle_ThrowsArgumentException()
    {
        var request = new CreateTaskRequest("   ", null);

        await Assert.ThrowsAsync<ArgumentException>(
            () => _sut.CreateAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsMappedResponse()
    {
        var existing = TaskItem.Create("Tarefa existente", "Descrição");
        var id = existing.Id;

        _repositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await _sut.GetByIdAsync(id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("Tarefa existente", result.Title);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        var id = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem?)null);

        var result = await _sut.GetByIdAsync(id, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_AnyId_QueriesRepositoryExactlyOnce()
    {
        var id = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem?)null);

        await _sut.GetByIdAsync(id, CancellationToken.None);

        _repositoryMock.Verify(
            r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_NoFilter_ReturnsAllTasks()
    {
        var tasks = new List<TaskItem>
        {
            TaskItem.Create("Tarefa A", null),
            TaskItem.Create("Tarefa B", "desc"),
            TaskItem.Create("Tarefa C", null),
        };

        _repositoryMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tasks);

        var result = await _sut.GetAllAsync(null, CancellationToken.None);

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetAllAsync_WithStatusFilter_PassesFilterToRepository()
    {
        var filtered = new List<TaskItem> { TaskItem.Create("Tarefa InProgress", null) };

        _repositoryMock
            .Setup(r => r.GetAllAsync(TaskItemStatus.InProgress, It.IsAny<CancellationToken>()))
            .ReturnsAsync(filtered);

        var result = await _sut.GetAllAsync(TaskItemStatus.InProgress, CancellationToken.None);

        Assert.Single(result);

        _repositoryMock.Verify(
            r => r.GetAllAsync(TaskItemStatus.InProgress, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_EmptyRepository_ReturnsEmptyList()
    {
        _repositoryMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TaskItem>());

        var result = await _sut.GetAllAsync(null, CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_MapsAllFieldsCorrectly()
    {
        var task = TaskItem.Create("Titulo mapeado", "Descricao mapeada");
        task.ChangeStatus(TaskItemStatus.InProgress);

        _repositoryMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TaskItem> { task });

        var result = await _sut.GetAllAsync(null, CancellationToken.None);

        var mapped = result.First();
        Assert.Equal(task.Id, mapped.Id);
        Assert.Equal("Titulo mapeado", mapped.Title);
        Assert.Equal("Descricao mapeada", mapped.Description);
        Assert.Equal((int)TaskItemStatus.InProgress, mapped.Status);
    }

    [Fact]
    public async Task UpdateAsync_ExistingTask_UpdatesTitleDescriptionAndStatus()
    {
        var existing = TaskItem.Create("Titulo antigo", "Desc antiga");
        var id = existing.Id;
        var request = new UpdateTaskRequest("Titulo novo", "Desc nova", (int)TaskItemStatus.InProgress);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        _repositoryMock
            .Setup(r => r.Update(It.IsAny<TaskItem>()));

        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(1));

        await _sut.UpdateAsync(id, request, CancellationToken.None);

        _repositoryMock.Verify(
            r => r.Update(It.Is<TaskItem>(t =>
                t.Title == "Titulo novo" &&
                t.Description == "Desc nova" &&
                t.Status == TaskItemStatus.InProgress)),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingTask_ThrowsKeyNotFoundException()
    {
        var id = Guid.NewGuid();
        var request = new UpdateTaskRequest("Titulo", null, (int)TaskItemStatus.Done);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.UpdateAsync(id, request, CancellationToken.None));

        _repositoryMock.Verify(r => r.Update(It.IsAny<TaskItem>()), Times.Never);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ExistingTask_CallsSaveChangesOnce()
    {
        var existing = TaskItem.Create("Titulo", null);
        var request = new UpdateTaskRequest("Novo titulo", null, (int)TaskItemStatus.Pending);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(existing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        _repositoryMock.Setup(r => r.Update(It.IsAny<TaskItem>()));

        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(1));

        await _sut.UpdateAsync(existing.Id, request, CancellationToken.None);

        _repositoryMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }


    [Fact]
    public async Task DeleteAsync_ExistingTask_CallsDeleteAndSave()
    {
        var existing = TaskItem.Create("Tarefa a deletar", null);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(existing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        _repositoryMock.Setup(r => r.Update(It.IsAny<TaskItem>()));

        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(1));

        await _sut.DeleteAsync(existing.Id, CancellationToken.None);

        _repositoryMock.Verify(r => r.Update(It.Is<TaskItem>(t => t.IsDeleted)), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingTask_ThrowsKeyNotFoundException()
    {
        var id = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.DeleteAsync(id, CancellationToken.None));

        _repositoryMock.Verify(r => r.Delete(It.IsAny<TaskItem>()), Times.Never);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ExistingTask_SoftDeletesInsteadOfHardDelete()
    {
        var existing = TaskItem.Create("Tarefa soft delete", null);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(existing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        _repositoryMock.Setup(r => r.Update(It.IsAny<TaskItem>()));
        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(1));

        await _sut.DeleteAsync(existing.Id, CancellationToken.None);

        _repositoryMock.Verify(
            r => r.Update(It.Is<TaskItem>(t => t.IsDeleted == true)),
            Times.Once);

        _repositoryMock.Verify(r => r.Delete(It.IsAny<TaskItem>()), Times.Never);
    }

    [Fact] 
    public async Task GetAllAsync_WithSoftDeletedTasks_DoesNotReturnDeleted()
    {

        var active = TaskItem.Create("Ativa", null);

        _repositoryMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TaskItem> { active }); 
        var result = await _sut.GetAllAsync(null, CancellationToken.None);

        Assert.Single(result); 
    }

    [Fact] 
    public async Task CreateAsync_TitleExceeds200Chars_ThrowsValidationException()
    {
        var longTitle = new string('a', 201);
        var request = new CreateTaskRequest(longTitle, null);

        await Assert.ThrowsAnyAsync<Exception>(() => _sut.CreateAsync(request, CancellationToken.None));

        _repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact] 
    public async Task UpdateAsync_InvalidStatusValue_ThrowsException()
    {
        
        var existing = TaskItem.Create("Tarefa", null);
        var request = new UpdateTaskRequest("Tarefa", null, 999);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(existing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        await Assert.ThrowsAnyAsync<Exception>(
            () => _sut.UpdateAsync(existing.Id, request, CancellationToken.None));
    }

    [Fact] 
    public async Task CreateAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var request = new CreateTaskRequest("Tarefa", null);

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _sut.CreateAsync(request, cts.Token));
    }
}