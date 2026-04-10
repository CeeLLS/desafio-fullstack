using Moq;
using TaskManager.Application.Abstractions;
using TaskManager.Application.Services;
using TaskManager.Application.Tasks.Contracts;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.UnitTests.Services;

/// <summary>
/// Convenção de nome: MetodoTestado_Cenario_ResultadoEsperado
/// Testes marcados com [Fact] //[Fact(Skip = "...")] representam comportamento FUTURO —
/// funcionalidade ainda não implementada mas planejada.
/// </summary>
public sealed class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _repositoryMock;
    private readonly TaskService _sut; // system under test

    public TaskServiceTests()
    {
        _repositoryMock = new Mock<ITaskRepository>(MockBehavior.Strict);
        _sut = new TaskService(_repositoryMock.Object);
    }

    // -------------------------------------------------------------------------
    // CreateAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsMappedResponse()
    {
        // Arrange
        var request = new CreateTaskRequest("Comprar café", "Café coado, não solúvel");

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(1));

        // Act
        var result = await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Comprar café", result.Title);
        Assert.Equal("Café coado, não solúvel", result.Description);
        Assert.Equal((int)TaskItemStatus.Pending, result.Status);
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_CallsAddAndSaveExactlyOnce()
    {
        // Arrange
        var request = new CreateTaskRequest("Titulo qualquer", null);

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(1));

        // Act
        await _sut.CreateAsync(request, CancellationToken.None);

        // Assert — verifica que o contrato com o repositório foi respeitado
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
        // Arrange
        var request = new CreateTaskRequest("Tarefa nova", null);

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(1));

        // Act
        var result = await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal((int)TaskItemStatus.Pending, result.Status);
    }

    [Fact]
    public async Task CreateAsync_TitleWithWhitespace_TrimsTitle()
    {
        // Arrange — o domínio deve fazer trim, testamos que o service mapeia correto
        var request = new CreateTaskRequest("  Titulo com espaço  ", null);

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(1));

        // Act
        var result = await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal("Titulo com espaço", result.Title);
    }

    [Fact]
    public async Task CreateAsync_EmptyTitle_ThrowsArgumentException()
    {
        // Arrange — TaskItem.Create lança ArgumentException para título vazio
        var request = new CreateTaskRequest("", null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _sut.CreateAsync(request, CancellationToken.None));

        // Verifica que nunca chegou ao repositório
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

    // -------------------------------------------------------------------------
    // GetByIdAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsMappedResponse()
    {
        // Arrange
        var existing = TaskItem.Create("Tarefa existente", "Descrição");
        var id = existing.Id;

        _repositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        // Act
        var result = await _sut.GetByIdAsync(id, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("Tarefa existente", result.Title);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem?)null);

        // Act
        var result = await _sut.GetByIdAsync(id, CancellationToken.None);

        // Assert
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

    // -------------------------------------------------------------------------
    // GetAllAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetAllAsync_NoFilter_ReturnsAllTasks()
    {
        // Arrange
        var tasks = new List<TaskItem>
        {
            TaskItem.Create("Tarefa A", null),
            TaskItem.Create("Tarefa B", "desc"),
            TaskItem.Create("Tarefa C", null),
        };

        _repositoryMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tasks);

        // Act
        var result = await _sut.GetAllAsync(null, CancellationToken.None);

        // Assert
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetAllAsync_WithStatusFilter_PassesFilterToRepository()
    {
        // Arrange
        var filtered = new List<TaskItem> { TaskItem.Create("Tarefa InProgress", null) };

        _repositoryMock
            .Setup(r => r.GetAllAsync(TaskItemStatus.InProgress, It.IsAny<CancellationToken>()))
            .ReturnsAsync(filtered);

        // Act
        var result = await _sut.GetAllAsync(TaskItemStatus.InProgress, CancellationToken.None);

        // Assert
        Assert.Single(result);

        // Verifica que o filtro foi repassado corretamente, não ignorado
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

    // -------------------------------------------------------------------------
    // UpdateAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task UpdateAsync_ExistingTask_UpdatesTitleDescriptionAndStatus()
    {
        // Arrange
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

        // Act
        await _sut.UpdateAsync(id, request, CancellationToken.None);

        // Assert — verifica que o objeto foi de fato modificado antes de Update ser chamado
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
        // Arrange
        var id = Guid.NewGuid();
        var request = new UpdateTaskRequest("Titulo", null, (int)TaskItemStatus.Done);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.UpdateAsync(id, request, CancellationToken.None));

        // Nenhum Update ou SaveChanges deve ter sido chamado
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

    // -------------------------------------------------------------------------
    // DeleteAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task DeleteAsync_ExistingTask_CallsDeleteAndSave()
    {
        // Arrange
        var existing = TaskItem.Create("Tarefa a deletar", null);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(existing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        _repositoryMock.Setup(r => r.Update(It.IsAny<TaskItem>()));

        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(1));

        // Act
        await _sut.DeleteAsync(existing.Id, CancellationToken.None);

        // Assert
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

    // -------------------------------------------------------------------------
    // Testes FUTUROS — representam funcionalidades planejadas que ainda não existem.
    // Vão falhar intencionalmente. Remova o Skip quando implementar.
    // -------------------------------------------------------------------------

    [Fact] //[Fact(Skip = "Futuro: soft delete — Delete deve marcar IsDeleted=true, não remover do banco")]
    public async Task DeleteAsync_ExistingTask_SoftDeletesInsteadOfHardDelete()
    {
        // Quando implementar soft delete, o repositório NÃO deve chamar Remove().
        // O TaskItem deve ter IsDeleted=true e o service chama Update(), não Delete().
        var existing = TaskItem.Create("Tarefa soft delete", null);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(existing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        _repositoryMock.Setup(r => r.Update(It.IsAny<TaskItem>()));
        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(1));

        await _sut.DeleteAsync(existing.Id, CancellationToken.None);

        // Deve chamar Update com IsDeleted=true
        _repositoryMock.Verify(
            r => r.Update(It.Is<TaskItem>(t => t.IsDeleted == true)),
            Times.Once);

        // Nunca deve chamar Delete físico
        _repositoryMock.Verify(r => r.Delete(It.IsAny<TaskItem>()), Times.Never);
    }

    [Fact] //[Fact(Skip = "Futuro: GetAllAsync não deve retornar tarefas com IsDeleted=true")]
    public async Task GetAllAsync_WithSoftDeletedTasks_DoesNotReturnDeleted()
    {
        // O repositório ou o query filter do EF deve filtrar automaticamente.
        // Esse teste valida o comportamento esperado do filtro global.
        var active = TaskItem.Create("Ativa", null);
        // Quando existir soft delete, haverá uma tarefa deletada que não deve aparecer

        _repositoryMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TaskItem> { active }); // repositório já filtra

        var result = await _sut.GetAllAsync(null, CancellationToken.None);

        Assert.Single(result); // só a ativa
    }

    [Fact] //[Fact(Skip = "Futuro: título deve ter no máximo 200 caracteres — validação no service ou domínio")]
    public async Task CreateAsync_TitleExceeds200Chars_ThrowsValidationException()
    {
        var longTitle = new string('a', 201);
        var request = new CreateTaskRequest(longTitle, null);

        // Quando implementar, deve lançar antes de chegar no repositório
        await Assert.ThrowsAnyAsync<Exception>(() => _sut.CreateAsync(request, CancellationToken.None));

        _repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact] //[Fact(Skip = "Futuro: status inválido (ex: 999) deve lançar exceção, não cast silencioso")]
    public async Task UpdateAsync_InvalidStatusValue_ThrowsException()
    {
        // Hoje o cast (TaskItemStatus)999 não lança nada — é um bug silencioso.
        // Quando corrigir, adicionar validação no service ou no domínio.
        var existing = TaskItem.Create("Tarefa", null);
        var request = new UpdateTaskRequest("Tarefa", null, 999);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(existing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        await Assert.ThrowsAnyAsync<Exception>(
            () => _sut.UpdateAsync(existing.Id, request, CancellationToken.None));
    }

    [Fact] //[Fact(Skip = "Futuro: CancellationToken cancelado deve propagar OperationCanceledException")]
    public async Task CreateAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var request = new CreateTaskRequest("Tarefa", null);

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _sut.CreateAsync(request, cts.Token));
    }
}