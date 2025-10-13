using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskApiService.Data;

namespace TaskApiService.Tests.Integration;

public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    public string DatabaseName { get; } = $"TestDb_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<TaskContext>));
            if (descriptor != null)
                services.Remove(descriptor);

                // Add test database with unique name for this factory instance
                services.AddDbContext<TaskContext>(options =>
                    options.UseInMemoryDatabase(DatabaseName));

                // Replace TaskContext with a test version that doesn't seed data
                services.AddScoped<TaskContext>(provider =>
                {
                    var options = provider.GetRequiredService<DbContextOptions<TaskContext>>();
                    return new TestTaskContext(options);
                });
        });

        builder.UseEnvironment("Testing");
    }

    public void SeedDatabase(Action<TaskContext> seedAction)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TaskContext>();
        context.Database.EnsureCreated();
        seedAction(context);
        context.SaveChanges();
    }
}
