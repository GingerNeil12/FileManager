using FileManager.WebApi.Option;

namespace FileManager.WebApi.Tests.Option;

[TestFixture]
public class CorsOptionsTests
{
    [Test]
    public void AllowedOrigins_WhenDefaultConstructed_IsEmpty()
    {
        // Arrange
        var sut = new CorsOptions();

        // Act / Assert
        Assert.That(sut.AllowedOrigins, Is.Empty);
    }

    [Test]
    public void AllowedOrigins_WhenSet_RetainsValue()
    {
        // Arrange
        var sut = new CorsOptions();
        string[] origins = ["http://localhost:4200", "http://localhost"];

        // Act
        sut.AllowedOrigins = origins;

        // Assert
        Assert.That(sut.AllowedOrigins, Is.EqualTo(origins));
    }
}
