using Microsoft.EntityFrameworkCore;
using TaskApiService.Data;
using TaskApiService.Models;

namespace TaskApiService.Tests.Integration;

public class TestTaskContext : TaskContext
{
    public TestTaskContext(DbContextOptions<TaskContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure the model but don't seed any data
        // Don't call base.OnModelCreating to avoid seeding sample data
    }
}
