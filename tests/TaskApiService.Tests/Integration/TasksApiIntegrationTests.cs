using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TaskApiService.Data;
using TaskApiService.Models;
using TaskApiService.Tests.Helpers;

namespace TaskApiService.Tests.Integration;

public class TasksApiIntegrationTests : IDisposable
{
    private readonly JsonSerializerOptions _jsonOptions;

    public TasksApiIntegrationTests()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    private CustomWebApplicationFactory<Program> CreateFactory()
    {
        return new CustomWebApplicationFactory<Program>();
    }

    [Fact]
    public async Task GetTasks_WithEmptyDatabase_ReturnsEmptyArray()
    {
        // Arrange
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/tasks");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var tasks = await response.Content.ReadFromJsonAsync<TaskItem[]>(_jsonOptions);
        tasks.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task PostTask_WithValidData_CreatesTask()
    {
        // Arrange
        using var factory = CreateFactory();
        using var client = factory.CreateClient();
        
        var createRequest = TestDataBuilder.CreateTaskRequest(
            title: "Integration Test Task",
            description: "Created via integration test",
            priority: TaskPriority.High);

        // Act
        var response = await client.PostAsJsonAsync("/api/tasks", createRequest, _jsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdTask = await response.Content.ReadFromJsonAsync<TaskItem>(_jsonOptions);
        
        createdTask.Should().NotBeNull();
        createdTask!.Title.Should().Be("Integration Test Task");
        createdTask.Description.Should().Be("Created via integration test");
        createdTask.Priority.Should().Be(TaskPriority.High);
        createdTask.IsCompleted.Should().BeFalse();
        createdTask.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetTask_WithValidId_ReturnsTask()
    {
        // Arrange
        using var factory = CreateFactory();
        using var client = factory.CreateClient();
        
        var createRequest = TestDataBuilder.CreateTaskRequest("Task to Get");
        var createResponse = await client.PostAsJsonAsync("/api/tasks", createRequest, _jsonOptions);
        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskItem>(_jsonOptions);

        // Act
        var response = await client.GetAsync($"/api/tasks/{createdTask!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var retrievedTask = await response.Content.ReadFromJsonAsync<TaskItem>(_jsonOptions);
        retrievedTask.Should().NotBeNull();
        retrievedTask!.Id.Should().Be(createdTask.Id);
        retrievedTask.Title.Should().Be("Task to Get");
    }

    [Fact]
    public async Task GetTask_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/tasks/999999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PutTask_WithValidData_UpdatesTask()
    {
        // Arrange
        using var factory = CreateFactory();
        using var client = factory.CreateClient();
        
        var createRequest = TestDataBuilder.CreateTaskRequest("Original Title");
        var createResponse = await client.PostAsJsonAsync("/api/tasks", createRequest, _jsonOptions);
        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskItem>(_jsonOptions);

        var updateRequest = TestDataBuilder.CreateUpdateTaskRequest(
            title: "Updated Title",
            isCompleted: true,
            priority: TaskPriority.Critical);

        // Act
        var response = await client.PutAsJsonAsync($"/api/tasks/{createdTask!.Id}", updateRequest, _jsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedTask = await response.Content.ReadFromJsonAsync<TaskItem>(_jsonOptions);
        
        updatedTask.Should().NotBeNull();
        updatedTask!.Title.Should().Be("Updated Title");
        updatedTask.IsCompleted.Should().BeTrue();
        updatedTask.Priority.Should().Be(TaskPriority.Critical);
        updatedTask.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteTask_WithValidId_DeletesTask()
    {
        // Arrange
        using var factory = CreateFactory();
        using var client = factory.CreateClient();
        
        var createRequest = TestDataBuilder.CreateTaskRequest("Task to Delete");
        var createResponse = await client.PostAsJsonAsync("/api/tasks", createRequest, _jsonOptions);
        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskItem>(_jsonOptions);

        // Act
        var deleteResponse = await client.DeleteAsync($"/api/tasks/{createdTask!.Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify task is deleted
        var getResponse = await client.GetAsync($"/api/tasks/{createdTask.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PostCompleteTask_WithValidId_CompletesTask()
    {
        // Arrange
        using var factory = CreateFactory();
        using var client = factory.CreateClient();
        
        var createRequest = TestDataBuilder.CreateTaskRequest("Task to Complete");
        var createResponse = await client.PostAsJsonAsync("/api/tasks", createRequest, _jsonOptions);
        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskItem>(_jsonOptions);

        // Act
        var response = await client.PostAsync($"/api/tasks/{createdTask!.Id}/complete", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var completedTask = await response.Content.ReadFromJsonAsync<TaskItem>(_jsonOptions);
        
        completedTask.Should().NotBeNull();
        completedTask!.IsCompleted.Should().BeTrue();
        completedTask.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task GetHealth_ReturnsHealthStatus()
    {
        // Arrange
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var healthContent = await response.Content.ReadAsStringAsync();
        healthContent.Should().Contain("Healthy");
    }
}