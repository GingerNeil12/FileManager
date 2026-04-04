using FileManager.WebApi.Controllers;
using FileManager.WebApi.DTOs.Common;
using FileManager.WebApi.Option;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FileManager.WebApi.Tests.Controllers;

[TestFixture]
public class VersionControllerTests
{
    [Test]
    public void GetVersion_WhenCalled_ShouldReturnNameAndVersion()
    {
        // Arrange
        var applicationInfo = new ApplicationInfo
        {
            Name = "filemanager-backend",
            Version = "1.0.0"
        };
        var sut = new VersionController(Options.Create(applicationInfo));

        // Act
        IActionResult result = sut.GetVersion();

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okObjectResult = result as OkObjectResult;
        Assert.That(okObjectResult!.Value, Is.InstanceOf<VersionDto>());
        var versionDto = okObjectResult.Value as VersionDto;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(versionDto!.Name, Is.EqualTo(applicationInfo.Name));
            Assert.That(versionDto!.Version, Is.EqualTo(applicationInfo.Version));
        }
    }
}