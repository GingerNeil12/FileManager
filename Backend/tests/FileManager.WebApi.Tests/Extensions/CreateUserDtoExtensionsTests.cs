#nullable disable

using FileManager.Application.Workflows.CreateUser;
using FileManager.Domain.Common.Enums;
using FileManager.WebApi.DTOs.Users;
using FileManager.WebApi.Extensions;

namespace FileManager.WebApi.Tests.Extensions;

[TestFixture]
public class CreateUserDtoExtensionsTests
{
    [Test]
    public void ToRequest_MapsEmailCorrectly()
    {
        // Arrange
        var dto = new CreateUserDto("jane@example.com", "Jane", "Doe", UserRoles.ExternalUser, null);

        // Act
        CreateUserRequest result = dto.ToRequest();

        // Assert
        Assert.That(result.Email, Is.EqualTo("jane@example.com"));
    }

    [Test]
    public void ToRequest_MapsGivenNameCorrectly()
    {
        // Arrange
        var dto = new CreateUserDto("jane@example.com", "Jane", "Doe", UserRoles.ExternalUser, null);

        // Act
        CreateUserRequest result = dto.ToRequest();

        // Assert
        Assert.That(result.GivenName, Is.EqualTo("Jane"));
    }

    [Test]
    public void ToRequest_MapsFamilyNameCorrectly()
    {
        // Arrange
        var dto = new CreateUserDto("jane@example.com", "Jane", "Doe", UserRoles.ExternalUser, null);

        // Act
        CreateUserRequest result = dto.ToRequest();

        // Assert
        Assert.That(result.FamilyName, Is.EqualTo("Doe"));
    }

    [Test]
    public void ToRequest_MapsRoleCorrectly()
    {
        // Arrange
        var dto = new CreateUserDto("jane@example.com", "Jane", "Doe", UserRoles.InternalAdmin, null);

        // Act
        CreateUserRequest result = dto.ToRequest();

        // Assert
        Assert.That(result.Role, Is.EqualTo(UserRoles.InternalAdmin));
    }

    [Test]
    public void ToRequest_WhenDepartmentIdIsProvided_MapsDepartmentIdCorrectly()
    {
        // Arrange
        var dto = new CreateUserDto("jane@example.com", "Jane", "Doe", UserRoles.InternalUser, 42);

        // Act
        CreateUserRequest result = dto.ToRequest();

        // Assert
        Assert.That(result.DepartmentId, Is.EqualTo(42));
    }

    [Test]
    public void ToRequest_WhenDepartmentIdIsNull_MapsNullDepartmentId()
    {
        // Arrange
        var dto = new CreateUserDto("jane@example.com", "Jane", "Doe", UserRoles.ExternalUser, null);

        // Act
        CreateUserRequest result = dto.ToRequest();

        // Assert
        Assert.That(result.DepartmentId, Is.Null);
    }
}
