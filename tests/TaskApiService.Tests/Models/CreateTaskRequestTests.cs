using System.ComponentModel.DataAnnotations;
using TaskApiService.Models;
using TaskApiService.Tests.Helpers;

namespace TaskApiService.Tests.Models;

public class CreateTaskRequestTests
{
    [Fact]
    public void CreateTaskRequest_WithValidData_IsValid()
    {
        // Arrange
        var request = TestDataBuilder.CreateTaskRequest(
            title: "Valid Task",
            description: "Valid Description",
            priority: TaskPriority.High);

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        validationResults.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void CreateTaskRequest_WithInvalidTitle_IsInvalid(string? invalidTitle)
    {
        // Arrange
        var request = TestDataBuilder.CreateTaskRequest(title: invalidTitle!);

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        validationResults.Should().NotBeEmpty();
        validationResults.Should().Contain(v => v.MemberNames.Contains(nameof(CreateTaskRequest.Title)));
    }

    [Fact]
    public void CreateTaskRequest_WithTooLongTitle_IsInvalid()
    {
        // Arrange
        var longTitle = new string('A', 201); // Exceeds 200 character limit
        var request = TestDataBuilder.CreateTaskRequest(title: longTitle);

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        validationResults.Should().NotBeEmpty();
        validationResults.Should().Contain(v => v.MemberNames.Contains(nameof(CreateTaskRequest.Title)));
    }

    [Fact]
    public void CreateTaskRequest_WithTooLongDescription_IsInvalid()
    {
        // Arrange
        var longDescription = new string('B', 1001); // Exceeds 1000 character limit
        var request = TestDataBuilder.CreateTaskRequest(description: longDescription);

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        validationResults.Should().NotBeEmpty();
        validationResults.Should().Contain(v => v.MemberNames.Contains(nameof(CreateTaskRequest.Description)));
    }

    [Fact]
    public void CreateTaskRequest_WithNullDescription_IsValid()
    {
        // Arrange
        var request = TestDataBuilder.CreateTaskRequest(description: null);

        // Act
        var validationResults = ValidateModel(request);

        // Assert
        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void CreateTaskRequest_DefaultPriority_IsMedium()
    {
        // Act
        var request = new CreateTaskRequest
        {
            Title = "Test Task"
        };

        // Assert
        request.Priority.Should().Be(TaskPriority.Medium);
    }

    private static List<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model);
        Validator.TryValidateObject(model, validationContext, validationResults, true);
        return validationResults;
    }
}
