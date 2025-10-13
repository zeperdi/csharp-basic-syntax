using Microsoft.EntityFrameworkCore;
using TaskApiService.Models;

namespace TaskApiService.Data;

public class TaskContext : DbContext
{
    public TaskContext(DbContextOptions<TaskContext> options) : base(options)
    {
    }

    public DbSet<TaskItem> Tasks { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Seed some sample data
        modelBuilder.Entity<TaskItem>().HasData(
            new TaskItem
            {
                Id = 1,
                Title = "Complete project setup",
                Description = "Set up the initial project structure and dependencies",
                IsCompleted = true,
                Priority = TaskPriority.High,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                CompletedAt = DateTime.UtcNow.AddDays(-1)
            },
            new TaskItem
            {
                Id = 2,
                Title = "Implement user authentication",
                Description = "Add JWT token-based authentication to the API",
                IsCompleted = false,
                Priority = TaskPriority.Critical,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new TaskItem
            {
                Id = 3,
                Title = "Write unit tests",
                Description = "Create comprehensive unit tests for all controllers",
                IsCompleted = false,
                Priority = TaskPriority.Medium,
                CreatedAt = DateTime.UtcNow
            }
        );
    }
}



