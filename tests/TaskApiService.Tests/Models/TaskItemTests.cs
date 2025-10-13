using System.ComponentModel.DataAnnotations;
using TaskApiService.Models;
using TaskApiService.Tests.Helpers;

namespace TaskApiService.Tests.Models;

public class TaskItemTests
{
    [Fact]
    public void TaskItem_WithValidData_IsValid()
    {
        // Arrange
        var task = TestDataBuilder.CreateTaskItem(
            title: "Valid Task",
            description: "Valid Description",
            priority: TaskPriority.High);

        // Act
        var validationResults = ValidateModel(task);

        // Assert
        validationResults.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void TaskItem_WithInvalidTitle_IsInvalid(string? invalidTitle)
    {
        // Arrange
        var task = TestDataBuilder.CreateTaskItem(title: invalidTitle!);

        // Act
        var validationResults = ValidateModel(task);

        // Assert
        validationResults.Should().NotBeEmpty();
        validationResults.Should().Contain(v => v.MemberNames.Contains(nameof(TaskItem.Title)));
    }

    [Fact]
    public void TaskItem_WithTooLongTitle_IsInvalid()
    {
        // Arrange
        var longTitle = new string('A', 201); // Exceeds 200 character limit
        var task = TestDataBuilder.CreateTaskItem(title: longTitle);

        // Act
        var validationResults = ValidateModel(task);

        // Assert
        validationResults.Should().NotBeEmpty();
        validationResults.Should().Contain(v => v.MemberNames.Contains(nameof(TaskItem.Title)));
    }

    [Fact]
    public void TaskItem_WithTooLongDescription_IsInvalid()
    {
        // Arrange
        var longDescription = new string('B', 1001); // Exceeds 1000 character limit
        var task = TestDataBuilder.CreateTaskItem(description: longDescription);

        // Act
        var validationResults = ValidateModel(task);

        // Assert
        validationResults.Should().NotBeEmpty();
        validationResults.Should().Contain(v => v.MemberNames.Contains(nameof(TaskItem.Description)));
    }

    [Fact]
    public void TaskItem_WithNullDescription_IsValid()
    {
        // Arrange
        var task = TestDataBuilder.CreateTaskItem(description: null);

        // Act
        var validationResults = ValidateModel(task);

        // Assert
        validationResults.Should().BeEmpty();
    }

    [Theory]
    [InlineData(TaskPriority.Low)]
    [InlineData(TaskPriority.Medium)]
    [InlineData(TaskPriority.High)]
    [InlineData(TaskPriority.Critical)]
    public void TaskItem_WithValidPriority_IsValid(TaskPriority priority)
    {
        // Arrange
        var task = TestDataBuilder.CreateTaskItem(priority: priority);

        // Act
        var validationResults = ValidateModel(task);

        // Assert
        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void TaskItem_DefaultValues_AreSetCorrectly()
    {
        // Act
        var task = new TaskItem
        {
            Title = "Test Task"
        };

        // Assert
        task.IsCompleted.Should().BeFalse();
        task.Priority.Should().Be(TaskPriority.Medium);
        task.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        task.CompletedAt.Should().BeNull();
        task.Description.Should().BeNull();
    }

    private static List<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model);
        Validator.TryValidateObject(model, validationContext, validationResults, true);
        return validationResults;
    }
}
