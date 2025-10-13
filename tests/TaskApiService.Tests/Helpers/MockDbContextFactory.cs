using Microsoft.EntityFrameworkCore;
using TaskApiService.Data;
using TaskApiService.Models;

namespace TaskApiService.Tests.Helpers;

public static class MockDbContextFactory
{
    public static TaskContext CreateInMemoryContext(string databaseName = null!)
    {
        databaseName ??= Guid.NewGuid().ToString();
        
        var options = new DbContextOptionsBuilder<TaskContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new TaskContext(options);
    }

    public static TaskContext CreateContextWithData(List<TaskItem>? tasks = null)
    {
        var context = CreateInMemoryContext();
        
        if (tasks != null)
        {
            context.Tasks.AddRange(tasks);
            context.SaveChanges();
        }

        return context;
    }
}
