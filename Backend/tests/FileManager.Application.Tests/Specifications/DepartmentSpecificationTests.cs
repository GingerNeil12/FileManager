using FileManager.Application.Common.Models.Specifications;
using FileManager.Domain.Models;

namespace FileManager.Application.Tests.Specifications;

[TestFixture]
public class DepartmentSpecificationTests
{
    [Test]
    public void Constructor_WithNameSearch_SetsCriteriaMatchingName()
    {
        // Arrange
        Department department = new() { Id = 1, Name = "Engineering" };

        // Act
        DepartmentSpecification sut = new("Engin", 1, 10);

        // Assert
        Assert.That(sut.Criteria, Is.Not.Null);
        Assert.That(sut.Criteria!.Compile()(department), Is.True);
    }

    [Test]
    public void Constructor_WithNameSearch_SetsCriteriaNotMatchingName()
    {
        // Arrange
        Department department = new() { Id = 1, Name = "Finance" };

        // Act
        DepartmentSpecification sut = new("Engin", 1, 10);

        // Assert
        Assert.That(sut.Criteria, Is.Not.Null);
        Assert.That(sut.Criteria!.Compile()(department), Is.False);
    }

    [Test]
    public void Constructor_WithNullNameSearch_LeavesNullCriteria()
    {
        // Arrange & Act
        DepartmentSpecification sut = new(null, 1, 10);

        // Assert
        Assert.That(sut.Criteria, Is.Null);
    }

    [Test]
    public void Constructor_WithWhitespaceNameSearch_LeavesNullCriteria()
    {
        // Arrange & Act
        DepartmentSpecification sut = new("   ", 1, 10);

        // Assert
        Assert.That(sut.Criteria, Is.Null);
    }

    [Test]
    public void Constructor_Always_SetsOrderByName()
    {
        // Arrange & Act
        DepartmentSpecification sut = new(null, 1, 10);

        // Assert
        Assert.That(sut.OrderBy, Is.Not.Null);
        Assert.That(sut.OrderByDescending, Is.Null);
    }

    [Test]
    public void Constructor_Always_AppliesPaging()
    {
        // Arrange & Act
        DepartmentSpecification sut = new(null, 3, 25);

        // Assert
        Assert.That(sut.PageNumber, Is.EqualTo(3));
        Assert.That(sut.PageSize, Is.EqualTo(25));
    }
}
