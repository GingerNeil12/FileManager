using System.Security.Claims;

using FileManager.Application.Common.Interfaces;
using FileManager.Domain.Common.Enums;
using FileManager.WebApi.Middleware;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NSubstitute;

namespace FileManager.WebApi.Tests.Middleware;

[TestFixture]
public class RequestLoggingMiddlewareTests
{
    private ICurrentUser _mockCurrentUser;
    private RequestDelegate _mockNext;
    private TestLogger<RequestLoggingMiddleware> _testLogger;
    private RequestLoggingMiddleware _sut;

    [SetUp]
    public void SetUp()
    {
        _mockCurrentUser = Substitute.For<ICurrentUser>();
        _mockNext = Substitute.For<RequestDelegate>();
        _testLogger = new TestLogger<RequestLoggingMiddleware>();
        _sut = new RequestLoggingMiddleware(_mockNext, _testLogger);
    }

    [Test]
    public async Task InvokeAsync_WhenUserIsAuthenticated_LogsAtInformationLevel()
    {
        // Arrange
        DefaultHttpContext context = BuildAuthenticatedContext(Guid.NewGuid(), UserRoles.ExternalUser, "/files");

        // Act
        await _sut.InvokeAsync(context, BuildServiceProvider());

        // Assert
        Assert.That(_testLogger.Entries, Has.Count.EqualTo(1));
        Assert.That(_testLogger.Entries[0].Level, Is.EqualTo(LogLevel.Information));
    }

    [Test]
    public async Task InvokeAsync_WhenUserIsAuthenticated_LogsUserId()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        _mockCurrentUser.GetUserId().Returns(userId);
        DefaultHttpContext context = BuildAuthenticatedContext(userId, UserRoles.ExternalUser, "/files");

        // Act
        await _sut.InvokeAsync(context, BuildServiceProvider());

        // Assert
        Assert.That(_testLogger.Entries[0].Message, Does.Contain(userId.ToString()));
    }

    [Test]
    public async Task InvokeAsync_WhenUserIsAuthenticated_LogsRole()
    {
        // Arrange
        _mockCurrentUser.GetRole().Returns(UserRoles.InternalAdmin);
        DefaultHttpContext context = BuildAuthenticatedContext(Guid.NewGuid(), UserRoles.InternalAdmin, "/files");

        // Act
        await _sut.InvokeAsync(context, BuildServiceProvider());

        // Assert
        Assert.That(_testLogger.Entries[0].Message, Does.Contain(UserRoles.InternalAdmin.ToString()));
    }

    [Test]
    public async Task InvokeAsync_WhenUserIsAuthenticated_LogsPath()
    {
        // Arrange
        DefaultHttpContext context = BuildAuthenticatedContext(Guid.NewGuid(), UserRoles.ExternalUser, "/files/123");

        // Act
        await _sut.InvokeAsync(context, BuildServiceProvider());

        // Assert
        Assert.That(_testLogger.Entries[0].Message, Does.Contain("/files/123"));
    }

    [Test]
    public async Task InvokeAsync_WhenUserIsNotAuthenticated_DoesNotLog()
    {
        // Arrange
        DefaultHttpContext context = BuildUnauthenticatedContext();

        // Act
        await _sut.InvokeAsync(context, BuildServiceProvider());

        // Assert
        Assert.That(_testLogger.Entries, Is.Empty);
    }

    [Test]
    public async Task InvokeAsync_WhenUserIsAuthenticated_CallsNext()
    {
        // Arrange
        DefaultHttpContext context = BuildAuthenticatedContext(Guid.NewGuid(), UserRoles.ExternalUser, "/files");

        // Act
        await _sut.InvokeAsync(context, BuildServiceProvider());

        // Assert
        await _mockNext.Received(1).Invoke(Arg.Is<HttpContext>(c => c == context));
    }

    [Test]
    public async Task InvokeAsync_WhenUserIsNotAuthenticated_CallsNext()
    {
        // Arrange
        DefaultHttpContext context = BuildUnauthenticatedContext();

        // Act
        await _sut.InvokeAsync(context, BuildServiceProvider());

        // Assert
        await _mockNext.Received(1).Invoke(Arg.Is<HttpContext>(c => c == context));
    }

    private IServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton(_mockCurrentUser);
        return services.BuildServiceProvider();
    }

    private static DefaultHttpContext BuildAuthenticatedContext(Guid userId, UserRoles role, string path)
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "auth0|12345") };
        var identity = new ClaimsIdentity(claims, authenticationType: "Bearer");
        var principal = new ClaimsPrincipal(identity);
        var context = new DefaultHttpContext { User = principal };
        context.Request.Path = path;
        context.Items[ApplicationConstants.CURRENT_USER_ID] = userId;
        return context;
    }

    private static DefaultHttpContext BuildUnauthenticatedContext()
        => new() { User = new ClaimsPrincipal(new ClaimsIdentity()) };

    private sealed class TestLogger<T> : ILogger<T>
    {
        public List<(LogLevel Level, string Message)> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Entries.Add((logLevel, formatter(state, exception)));
        }
    }
}
