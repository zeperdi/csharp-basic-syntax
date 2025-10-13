using TaskApiService.Models;

namespace TaskApiService.Tests.Helpers;

public static class TestDataBuilder
{
    public static TaskItem CreateTaskItem(
        int id = 1,
        string title = "Test Task",
        string? description = "Test Description",
        bool isCompleted = false,
        TaskPriority priority = TaskPriority.Medium,
        DateTime? createdAt = null,
        DateTime? completedAt = null)
    {
        return new TaskItem
        {
            Id = id,
            Title = title,
            Description = description,
            IsCompleted = isCompleted,
            Priority = priority,
            CreatedAt = createdAt ?? DateTime.UtcNow,
            CompletedAt = completedAt
        };
    }

    public static CreateTaskRequest CreateTaskRequest(
        string title = "Test Task",
        string? description = "Test Description",
        TaskPriority priority = TaskPriority.Medium)
    {
        return new CreateTaskRequest
        {
            Title = title,
            Description = description,
            Priority = priority
        };
    }

    public static UpdateTaskRequest CreateUpdateTaskRequest(
        string? title = null,
        string? description = null,
        bool? isCompleted = null,
        TaskPriority? priority = null)
    {
        return new UpdateTaskRequest
        {
            Title = title,
            Description = description,
            IsCompleted = isCompleted,
            Priority = priority
        };
    }

    public static List<TaskItem> CreateTaskList()
    {
        return new List<TaskItem>
        {
            CreateTaskItem(1, "First Task", "First Description", false, TaskPriority.High),
            CreateTaskItem(2, "Second Task", "Second Description", true, TaskPriority.Low, completedAt: DateTime.UtcNow.AddDays(-1)),
            CreateTaskItem(3, "Third Task", "Third Description", false, TaskPriority.Critical),
            CreateTaskItem(4, "Fourth Task", "Fourth Description", true, TaskPriority.Medium, completedAt: DateTime.UtcNow.AddHours(-2))
        };
    }
}
