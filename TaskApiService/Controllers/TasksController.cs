using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskApiService.Data;
using TaskApiService.Models;

namespace TaskApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly TaskContext _context;
    private readonly ILogger<TasksController> _logger;

    public TasksController(TaskContext context, ILogger<TasksController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all tasks
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasks(
        [FromQuery] bool? completed = null,
        [FromQuery] TaskPriority? priority = null)
    {
        try
        {
            var query = _context.Tasks.AsQueryable();

            if (completed.HasValue)
            {
                query = query.Where(t => t.IsCompleted == completed.Value);
            }

            if (priority.HasValue)
            {
                query = query.Where(t => t.Priority == priority.Value);
            }

            var tasks = await query.OrderBy(t => t.CreatedAt).ToListAsync();
            return Ok(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tasks");
            return StatusCode(500, "An error occurred while retrieving tasks");
        }
    }

    /// <summary>
    /// Get a specific task by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<TaskItem>> GetTask(int id)
    {
        try
        {
            var task = await _context.Tasks.FindAsync(id);

            if (task == null)
            {
                return NotFound($"Task with ID {id} not found");
            }

            return Ok(task);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving task {TaskId}", id);
            return StatusCode(500, "An error occurred while retrieving the task");
        }
    }

    /// <summary>
    /// Create a new task
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<TaskItem>> CreateTask(CreateTaskRequest request)
    {
        try
        {
            var task = new TaskItem
            {
                Title = request.Title,
                Description = request.Description,
                Priority = request.Priority,
                CreatedAt = DateTime.UtcNow
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task");
            return StatusCode(500, "An error occurred while creating the task");
        }
    }

    /// <summary>
    /// Update an existing task
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTask(int id, UpdateTaskRequest request)
    {
        try
        {
            var task = await _context.Tasks.FindAsync(id);

            if (task == null)
            {
                return NotFound($"Task with ID {id} not found");
            }

            // Update only provided fields
            if (!string.IsNullOrEmpty(request.Title))
            {
                task.Title = request.Title;
            }

            if (request.Description != null)
            {
                task.Description = request.Description;
            }

            if (request.IsCompleted.HasValue)
            {
                task.IsCompleted = request.IsCompleted.Value;
                if (request.IsCompleted.Value && task.CompletedAt == null)
                {
                    task.CompletedAt = DateTime.UtcNow;
                }
                else if (!request.IsCompleted.Value)
                {
                    task.CompletedAt = null;
                }
            }

            if (request.Priority.HasValue)
            {
                task.Priority = request.Priority.Value;
            }

            await _context.SaveChangesAsync();

            return Ok(task);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task {TaskId}", id);
            return StatusCode(500, "An error occurred while updating the task");
        }
    }

    /// <summary>
    /// Delete a task
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        try
        {
            var task = await _context.Tasks.FindAsync(id);

            if (task == null)
            {
                return NotFound($"Task with ID {id} not found");
            }

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting task {TaskId}", id);
            return StatusCode(500, "An error occurred while deleting the task");
        }
    }

    /// <summary>
    /// Mark a task as completed
    /// </summary>
    [HttpPost("{id}/complete")]
    public async Task<ActionResult<TaskItem>> CompleteTask(int id)
    {
        try
        {
            var task = await _context.Tasks.FindAsync(id);

            if (task == null)
            {
                return NotFound($"Task with ID {id} not found");
            }

            if (task.IsCompleted)
            {
                return BadRequest("Task is already completed");
            }

            task.IsCompleted = true;
            task.CompletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(task);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing task {TaskId}", id);
            return StatusCode(500, "An error occurred while completing the task");
        }
    }
}



