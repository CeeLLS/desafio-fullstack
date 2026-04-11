using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.Services;
using TaskManager.Application.Tasks.Contracts;
using TaskManager.Domain.Enums;
 
namespace TaskManager.API.Controllers;
 
[ApiController]
[Route("api/[controller]")]
public sealed class TasksController : ControllerBase
{
    private readonly ITaskService _service;
 
    public TasksController(ITaskService service)
    {
        _service = service;
    }
 
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TaskResponse>>> GetAll(
        [FromQuery] TaskItemStatus? status,
        CancellationToken ct)
        => Ok(await _service.GetAllAsync(status, ct));
 
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TaskResponse>> GetById(Guid id, CancellationToken ct)
    {
        var task = await _service.GetByIdAsync(id, ct);
        return task is null ? NotFound() : Ok(task);
    }
 
    [HttpPost]
    public async Task<ActionResult<TaskResponse>> Create(
        CreateTaskRequest request,
        CancellationToken ct)
    {
        var created = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }
 
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateTaskRequest request,
        CancellationToken ct)
    {
        await _service.UpdateAsync(id, request, ct);
        return NoContent();
    }
 
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }
 
    [HttpPatch("{id:guid}/restore")]
    public async Task<IActionResult> Restore(Guid id, CancellationToken ct)
    {
        await _service.RestoreAsync(id, ct);
        return NoContent();
    }
}
 