using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TaskManager.Application.Services;
using TaskManager.Application.Tasks.Contracts;
using TaskManager.Domain.Enums;

namespace TaskManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[SwaggerTag("Gerenciamento de tarefas")]
public sealed class TasksController : ControllerBase
{
    private readonly ITaskService _service;

    public TasksController(ITaskService service) => _service = service;

    [HttpGet]
    [SwaggerOperation(
        Summary = "Lista tarefas ativas",
        Description = "Retorna todas as tarefas não deletadas. Aceita filtro opcional por status.")]
    [SwaggerResponse(200, "Lista de tarefas", typeof(IReadOnlyList<TaskResponse>))]
    public async Task<ActionResult<IReadOnlyList<TaskResponse>>> GetAll(
        [FromQuery, SwaggerParameter("0=Pending, 1=InProgress, 2=Done, 3=Canceled")]
        TaskItemStatus? status,
        CancellationToken ct)
        => Ok(await _service.GetAllAsync(status, ct));

    [HttpGet("deleted")]
    [SwaggerOperation(
        Summary = "Lista tarefas removidas",
        Description = "Retorna tarefas com soft delete aplicado, disponíveis para restauro.")]
    [SwaggerResponse(200, "Lista de tarefas removidas", typeof(IReadOnlyList<TaskResponse>))]
    public async Task<ActionResult<IReadOnlyList<TaskResponse>>> GetDeleted(CancellationToken ct)
        => Ok(await _service.GetDeletedAsync(ct));

    [HttpGet("{id:guid}")]
    [SwaggerOperation(Summary = "Busca tarefa por id")]
    [SwaggerResponse(200, "Tarefa encontrada", typeof(TaskResponse))]
    [SwaggerResponse(404, "Tarefa não encontrada")]
    public async Task<ActionResult<TaskResponse>> GetById(Guid id, CancellationToken ct)
    {
        var task = await _service.GetByIdAsync(id, ct);
        return task is null ? NotFound() : Ok(task);
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Cria uma nova tarefa")]
    [SwaggerResponse(201, "Tarefa criada", typeof(TaskResponse))]
    [SwaggerResponse(400, "Dados inválidos")]
    public async Task<ActionResult<TaskResponse>> Create(
        CreateTaskRequest request, CancellationToken ct)
    {
        var created = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [SwaggerOperation(Summary = "Atualiza título, descrição e status de uma tarefa")]
    [SwaggerResponse(204, "Tarefa atualizada")]
    [SwaggerResponse(400, "Dados inválidos")]
    [SwaggerResponse(404, "Tarefa não encontrada")]
    public async Task<IActionResult> Update(
        Guid id, UpdateTaskRequest request, CancellationToken ct)
    {
        await _service.UpdateAsync(id, request, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [SwaggerOperation(
        Summary = "Remove uma tarefa (soft delete)",
        Description = "Marca a tarefa como deletada sem removê-la do banco. Pode ser restaurada via PATCH /restore.")]
    [SwaggerResponse(204, "Tarefa removida")]
    [SwaggerResponse(404, "Tarefa não encontrada")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/restore")]
    [SwaggerOperation(
        Summary = "Restaura uma tarefa removida",
        Description = "Reverte o soft delete. A tarefa volta ao status Pending e reaparece na listagem padrão.")]
    [SwaggerResponse(204, "Tarefa restaurada")]
    [SwaggerResponse(404, "Tarefa não encontrada")]
    public async Task<IActionResult> Restore(Guid id, CancellationToken ct)
    {
        await _service.RestoreAsync(id, ct);
        return NoContent();
    }
}