using System.ComponentModel.DataAnnotations;

namespace TaskApiService.Models;

public class CreateTaskRequest
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
}

public class UpdateTaskRequest
{
    [StringLength(200)]
    public string? Title { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    public bool? IsCompleted { get; set; }

    public TaskPriority? Priority { get; set; }
}



