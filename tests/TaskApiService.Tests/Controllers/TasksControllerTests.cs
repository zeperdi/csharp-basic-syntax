using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TaskApiService.Controllers;
using TaskApiService.Data;
using TaskApiService.Models;
using TaskApiService.Tests.Helpers;

namespace TaskApiService.Tests.Controllers;

public class TasksControllerTests : IDisposable
{
    private readonly TaskContext _context;
    private readonly TasksController _controller;
    private readonly Mock<ILogger<TasksController>> _mockLogger;

    public TasksControllerTests()
    {
        _context = MockDbContextFactory.CreateInMemoryContext();
        _mockLogger = new Mock<ILogger<TasksController>>();
        _controller = new TasksController(_context, _mockLogger.Object);
    }

    public void Dispose()
    {
        _context?.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task GetTasks_WithNoTasks_ReturnsEmptyList()
    {
        // Act
        var result = await _controller.GetTasks();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var tasks = okResult.Value.Should().BeAssignableTo<IEnumerable<TaskItem>>().Subject;
        tasks.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTasks_WithTasks_ReturnsAllTasks()
    {
        // Arrange
        var testTasks = TestDataBuilder.CreateTaskList();
        await _context.Tasks.AddRangeAsync(testTasks);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetTasks();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var tasks = okResult.Value.Should().BeAssignableTo<IEnumerable<TaskItem>>().Subject;
        tasks.Should().HaveCount(4);
    }

    [Theory]
    [InlineData(true, 2)]  // 2 completed tasks
    [InlineData(false, 2)] // 2 incomplete tasks
    public async Task GetTasks_WithCompletedFilter_ReturnsFilteredTasks(bool completed, int expectedCount)
    {
        // Arrange
        var testTasks = TestDataBuilder.CreateTaskList();
        await _context.Tasks.AddRangeAsync(testTasks);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetTasks(completed: completed);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var tasks = okResult.Value.Should().BeAssignableTo<IEnumerable<TaskItem>>().Subject;
        tasks.Should().HaveCount(expectedCount);
        tasks.Should().AllSatisfy(t => t.IsCompleted.Should().Be(completed));
    }

    [Theory]
    [InlineData(TaskPriority.High, 1)]
    [InlineData(TaskPriority.Low, 1)]
    [InlineData(TaskPriority.Critical, 1)]
    [InlineData(TaskPriority.Medium, 1)]
    public async Task GetTasks_WithPriorityFilter_ReturnsFilteredTasks(TaskPriority priority, int expectedCount)
    {
        // Arrange
        var testTasks = TestDataBuilder.CreateTaskList();
        await _context.Tasks.AddRangeAsync(testTasks);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetTasks(priority: priority);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var tasks = okResult.Value.Should().BeAssignableTo<IEnumerable<TaskItem>>().Subject;
        tasks.Should().HaveCount(expectedCount);
        tasks.Should().AllSatisfy(t => t.Priority.Should().Be(priority));
    }

    [Fact]
    public async Task GetTask_WithValidId_ReturnsTask()
    {
        // Arrange
        var task = TestDataBuilder.CreateTaskItem(1, "Test Task");
        await _context.Tasks.AddAsync(task);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetTask(1);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedTask = okResult.Value.Should().BeOfType<TaskItem>().Subject;
        returnedTask.Id.Should().Be(1);
        returnedTask.Title.Should().Be("Test Task");
    }

    [Fact]
    public async Task GetTask_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var result = await _controller.GetTask(999);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task CreateTask_WithValidRequest_ReturnsCreatedTask()
    {
        // Arrange
        var request = TestDataBuilder.CreateTaskRequest("New Task", "New Description", TaskPriority.High);

        // Act
        var result = await _controller.CreateTask(request);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var createdTask = createdResult.Value.Should().BeOfType<TaskItem>().Subject;
        
        createdTask.Title.Should().Be("New Task");
        createdTask.Description.Should().Be("New Description");
        createdTask.Priority.Should().Be(TaskPriority.High);
        createdTask.IsCompleted.Should().BeFalse();
        createdTask.Id.Should().BeGreaterThan(0);
        createdTask.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task UpdateTask_WithValidId_UpdatesTask()
    {
        // Arrange
        var task = TestDataBuilder.CreateTaskItem(1, "Original Title");
        await _context.Tasks.AddAsync(task);
        await _context.SaveChangesAsync();

        var updateRequest = TestDataBuilder.CreateUpdateTaskRequest(
            title: "Updated Title",
            isCompleted: true,
            priority: TaskPriority.Critical);

        // Act
        var result = await _controller.UpdateTask(1, updateRequest);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var updatedTask = okResult.Value.Should().BeOfType<TaskItem>().Subject;
        
        updatedTask.Title.Should().Be("Updated Title");
        updatedTask.IsCompleted.Should().BeTrue();
        updatedTask.Priority.Should().Be(TaskPriority.Critical);
        updatedTask.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task UpdateTask_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var updateRequest = TestDataBuilder.CreateUpdateTaskRequest(title: "Updated Title");

        // Act
        var result = await _controller.UpdateTask(999, updateRequest);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeleteTask_WithValidId_DeletesTask()
    {
        // Arrange
        var task = TestDataBuilder.CreateTaskItem(1, "Task to Delete");
        await _context.Tasks.AddAsync(task);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.DeleteTask(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        
        var deletedTask = await _context.Tasks.FindAsync(1);
        deletedTask.Should().BeNull();
    }

    [Fact]
    public async Task DeleteTask_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var result = await _controller.DeleteTask(999);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task CompleteTask_WithValidId_MarksTaskAsCompleted()
    {
        // Arrange
        var task = TestDataBuilder.CreateTaskItem(1, "Task to Complete", isCompleted: false);
        await _context.Tasks.AddAsync(task);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.CompleteTask(1);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var completedTask = okResult.Value.Should().BeOfType<TaskItem>().Subject;
        
        completedTask.IsCompleted.Should().BeTrue();
        completedTask.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task CompleteTask_WithAlreadyCompletedTask_ReturnsBadRequest()
    {
        // Arrange
        var task = TestDataBuilder.CreateTaskItem(1, "Already Completed", isCompleted: true, completedAt: DateTime.UtcNow.AddDays(-1));
        await _context.Tasks.AddAsync(task);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.CompleteTask(1);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CompleteTask_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var result = await _controller.CompleteTask(999);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }
}
