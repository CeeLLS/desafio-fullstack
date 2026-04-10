using Moq;
using TaskManager.Application.Abstractions;
using TaskManager.Application.Services;
using TaskManager.Domain.Entities;
using Xunit;

namespace TaskManager.UnitTests.Services;

public class TaskCleanupServiceTests
{
    private readonly Mock<ITaskRepository> _repositoryMock;
    private readonly TaskCleanupService _sut;

    public TaskCleanupServiceTests()
    {
        _repositoryMock = new Mock<ITaskRepository>(MockBehavior.Strict);
        _sut = new TaskCleanupService(_repositoryMock.Object);
    }

    [Fact]
    public void Constructor_NullRepository_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new TaskCleanupService(null!));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task CleanupSoftDeletedTasksAsync_InvalidRetentionDays_ThrowsArgumentException(int invalidDays)
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _sut.CleanupSoftDeletedTasksAsync(retentionDays: invalidDays));
    }

    [Fact]
    public async Task CleanupSoftDeletedTasksAsync_NoEligibleTasks_ReturnsZeroAndSkipsSave()
    {
        _repositoryMock
            .Setup(r => r.GetSoftDeletedOlderThanAsync(30, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TaskItem>());

        var result = await _sut.CleanupSoftDeletedTasksAsync(retentionDays: 30);

        Assert.Equal(0, result);
        _repositoryMock.Verify(r => r.Delete(It.IsAny<TaskItem>()), Times.Never);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CleanupSoftDeletedTasksAsync_WithEligibleTasks_DeletesAllAndSavesOnce()
    {
        var task1 = TaskItem.Create("Tarefa antiga 1", null);
        var task2 = TaskItem.Create("Tarefa antiga 2", null);
        task1.SoftDelete();
        task2.SoftDelete();

        _repositoryMock
            .Setup(r => r.GetSoftDeletedOlderThanAsync(30, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TaskItem> { task1, task2 });

        _repositoryMock.Setup(r => r.Delete(It.IsAny<TaskItem>()));

        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        var result = await _sut.CleanupSoftDeletedTasksAsync(retentionDays: 30);

        Assert.Equal(2, result);
        _repositoryMock.Verify(r => r.Delete(It.IsAny<TaskItem>()), Times.Exactly(2));
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CleanupSoftDeletedTasksAsync_PassesRetentionDaysToRepository()
    {
        _repositoryMock
            .Setup(r => r.GetSoftDeletedOlderThanAsync(90, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TaskItem>());

        await _sut.CleanupSoftDeletedTasksAsync(retentionDays: 90);

        _repositoryMock.Verify(
            r => r.GetSoftDeletedOlderThanAsync(90, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CleanupSoftDeletedTasksAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _sut.CleanupSoftDeletedTasksAsync(retentionDays: 30, cts.Token));
    }
}