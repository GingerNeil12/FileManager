using System.Net.Mime;
using System.Text.Json;

using FileManager.WebApi.Exceptions;
using FileManager.WebApi.Handlers;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using NSubstitute;

namespace FileManager.WebApi.Tests.Handlers;

[TestFixture]
public class GlobalExceptionHandlerTests
{
    private ILogger<GlobalExceptionHandler> _mockLogger;
    private GlobalExceptionHandler _sut;

    [SetUp]
    public void SetUp()
    {
        _mockLogger = Substitute.For<ILogger<GlobalExceptionHandler>>();
        _sut = new GlobalExceptionHandler(_mockLogger);
    }

    [Test]
    public async Task TryHandleAsync_WhenCurrentUserNotFoundException_Returns401WithProblemDetails()
    {
        // Arrange
        DefaultHttpContext httpContext = BuildHttpContext();
        var exception = new CurrentUserNotFoundException("auth0|12345");

        // Act
        await _sut.TryHandleAsync(httpContext, exception, CancellationToken.None);

        // Assert
        ProblemDetails? problemDetails = await ReadProblemDetails(httpContext.Response);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(httpContext.Response.StatusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));
            Assert.That(problemDetails!.Status, Is.EqualTo(StatusCodes.Status401Unauthorized));
            Assert.That(problemDetails.Title, Is.EqualTo("Unauthorized"));
            Assert.That(problemDetails.Detail, Is.EqualTo("User account not found."));
        }
    }

    [Test]
    public async Task TryHandleAsync_WhenUnhandledException_Returns500WithProblemDetails()
    {
        // Arrange
        DefaultHttpContext httpContext = BuildHttpContext();
        var exception = new Exception("Something went wrong.");

        // Act
        await _sut.TryHandleAsync(httpContext, exception, CancellationToken.None);

        // Assert
        ProblemDetails? problemDetails = await ReadProblemDetails(httpContext.Response);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(httpContext.Response.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
            Assert.That(problemDetails!.Status, Is.EqualTo(StatusCodes.Status500InternalServerError));
            Assert.That(problemDetails.Title, Is.EqualTo("Internal Server Error"));
            Assert.That(problemDetails.Detail, Is.EqualTo("An unhandled error occured. Try again later."));
        }
    }

    [Test]
    public async Task TryHandleAsync_WhenCurrentUserNotFoundException_LogsWarning()
    {
        // Arrange
        DefaultHttpContext httpContext = BuildHttpContext();
        var exception = new CurrentUserNotFoundException("auth0|12345");

        // Act
        await _sut.TryHandleAsync(httpContext, exception, CancellationToken.None);

        // Assert
        AssertLogged(LogLevel.Warning);
    }

    [Test]
    public async Task TryHandleAsync_WhenUnhandledException_LogsError()
    {
        // Arrange
        DefaultHttpContext httpContext = BuildHttpContext();
        var exception = new Exception("Something went wrong.");

        // Act
        await _sut.TryHandleAsync(httpContext, exception, CancellationToken.None);

        // Assert
        AssertLogged(LogLevel.Error);
    }

    [Test]
    public async Task TryHandleAsync_WhenCurrentUserNotFoundException_SetsProblemJsonContentType()
    {
        // Arrange
        DefaultHttpContext httpContext = BuildHttpContext();
        var exception = new CurrentUserNotFoundException("auth0|12345");

        // Act
        await _sut.TryHandleAsync(httpContext, exception, CancellationToken.None);

        // Assert
        Assert.That(httpContext.Response.ContentType, Does.Contain(MediaTypeNames.Application.ProblemJson));
    }

    [Test]
    public async Task TryHandleAsync_WhenUnhandledException_SetsProblemJsonContentType()
    {
        // Arrange
        DefaultHttpContext httpContext = BuildHttpContext();
        var exception = new Exception("Something went wrong.");

        // Act
        await _sut.TryHandleAsync(httpContext, exception, CancellationToken.None);

        // Assert
        Assert.That(httpContext.Response.ContentType, Does.Contain(MediaTypeNames.Application.ProblemJson));
    }

    [Test]
    public async Task TryHandleAsync_WhenCalled_ReturnsTrue()
    {
        // Arrange
        DefaultHttpContext httpContext = BuildHttpContext();
        var exception = new Exception("Something went wrong.");

        // Act
        bool result = await _sut.TryHandleAsync(httpContext, exception, CancellationToken.None);

        // Assert
        Assert.That(result, Is.True);
    }

    private static DefaultHttpContext BuildHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<ProblemDetails?> ReadProblemDetails(HttpResponse response)
    {
        response.Body.Seek(0, SeekOrigin.Begin);
        return await JsonSerializer.DeserializeAsync<ProblemDetails>(response.Body);
    }

    private void AssertLogged(LogLevel expectedLevel)
    {
        bool wasLogged = _mockLogger.ReceivedCalls()
            .Any(c => c.GetMethodInfo().Name == "Log"
                   && c.GetArguments()[0] is LogLevel level
                   && level == expectedLevel);
        Assert.That(wasLogged, Is.True);
    }
}
