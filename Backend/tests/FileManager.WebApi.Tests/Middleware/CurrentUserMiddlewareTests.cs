using System.Security.Claims;

using FileManager.Application.Common.Interfaces;
using FileManager.Domain.Common;
using FileManager.Domain.Common.Enums;
using FileManager.Domain.Common.Errors;
using FileManager.Domain.Models;
using FileManager.WebApi.Exceptions;
using FileManager.WebApi.Middleware;

using Microsoft.AspNetCore.Http;

using NSubstitute;

namespace FileManager.WebApi.Tests.Middleware;

[TestFixture]
public class CurrentUserMiddlewareTests
{
    private IApplicationUserRepository _mockRepo;
    private RequestDelegate _mockNext;
    private CurrentUserMiddleware _sut;

    [SetUp]
    public void SetUp()
    {
        _mockRepo = Substitute.For<IApplicationUserRepository>();
        _mockNext = Substitute.For<RequestDelegate>();
        _sut = new CurrentUserMiddleware(_mockNext);
    }

    [Test]
    public async Task InvokeAsync_WhenUserIsAuthenticatedAndFound_SetsUserIdInItems()
    {
        // Arrange
        ApplicationUser user = ApplicationUser.Create("auth0|12345", "email", "given", "family", UserRoles.ExternalUser, null);
        DefaultHttpContext context = BuildAuthenticatedContext("auth0|12345");
        SetupRepositorySuccess(user);

        // Act
        await _sut.InvokeAsync(context, _mockRepo);

        // Assert
        Assert.That(context.Items[ApplicationConstants.CURRENT_USER_ID], Is.EqualTo(user.Id));
    }

    [Test]
    public async Task InvokeAsync_WhenUserIsAuthenticatedAndFound_CallsNext()
    {
        // Arrange
        ApplicationUser user = ApplicationUser.Create("auth0|12345", "email", "given", "family", UserRoles.ExternalUser, null);
        DefaultHttpContext context = BuildAuthenticatedContext("auth0|12345");
        SetupRepositorySuccess(user);

        // Act
        await _sut.InvokeAsync(context, _mockRepo);

        // Assert
        await _mockNext.Received(1).Invoke(Arg.Is<HttpContext>(c => c == context));
    }

    [Test]
    public async Task InvokeAsync_WhenUserIsAuthenticatedAndFound_QueriesRepoByExternalProviderId()
    {
        // Arrange
        ApplicationUser user = ApplicationUser.Create("auth0|12345", "email", "given", "family", UserRoles.ExternalUser, null);
        DefaultHttpContext context = BuildAuthenticatedContext("auth0|12345");
        SetupRepositorySuccess(user);

        // Act
        await _sut.InvokeAsync(context, _mockRepo);

        // Assert
        await _mockRepo.Received(1).GetByAsync(
            Arg.Is<string>(id => id == "auth0|12345"),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public void InvokeAsync_WhenUserIsAuthenticatedAndNotFoundInDb_ThrowsCurrentUserNotFoundException()
    {
        // Arrange
        DefaultHttpContext context = BuildAuthenticatedContext("auth0|12345");
        SetupRepositoryFailure();

        // Act & Assert
        Assert.ThrowsAsync<CurrentUserNotFoundException>(() => _sut.InvokeAsync(context, _mockRepo));
    }

    [Test]
    public void InvokeAsync_WhenUserIsAuthenticatedAndBlocked_ThrowsUserBlockedException()
    {
        // Arrange
        ApplicationUser user = ApplicationUser.Create("auth0|12345", "email", "given", "family", UserRoles.ExternalUser, null);
        user.Disable();
        DefaultHttpContext context = BuildAuthenticatedContext("auth0|12345");
        SetupRepositorySuccess(user);

        // Act & Assert
        Assert.ThrowsAsync<UserBlockedException>(() => _sut.InvokeAsync(context, _mockRepo));
    }

    [Test]
    public async Task InvokeAsync_WhenUserIsNotAuthenticated_DoesNotQueryRepository()
    {
        // Arrange
        DefaultHttpContext context = BuildUnauthenticatedContext();

        // Act
        await _sut.InvokeAsync(context, _mockRepo);

        // Assert
        await _mockRepo.DidNotReceive().GetByAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task InvokeAsync_WhenUserIsNotAuthenticated_CallsNext()
    {
        // Arrange
        DefaultHttpContext context = BuildUnauthenticatedContext();

        // Act
        await _sut.InvokeAsync(context, _mockRepo);

        // Assert
        await _mockNext.Received(1).Invoke(Arg.Is<HttpContext>(c => c == context));
    }

    [Test]
    public async Task InvokeAsync_WhenUserIsNotAuthenticated_DoesNotSetUserIdInItems()
    {
        // Arrange
        DefaultHttpContext context = BuildUnauthenticatedContext();

        // Act
        await _sut.InvokeAsync(context, _mockRepo);

        // Assert
        Assert.That(context.Items.ContainsKey(ApplicationConstants.CURRENT_USER_ID), Is.False);
    }

    private static DefaultHttpContext BuildAuthenticatedContext(string externalProviderId)
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, externalProviderId) };
        var identity = new ClaimsIdentity(claims, authenticationType: "Bearer");
        var principal = new ClaimsPrincipal(identity);
        return new DefaultHttpContext { User = principal };
    }

    private static DefaultHttpContext BuildUnauthenticatedContext()
        => new() { User = new ClaimsPrincipal(new ClaimsIdentity()) };

    private void SetupRepositorySuccess(ApplicationUser user)
    {
        Result<ApplicationUser, Error> result = user;
        _mockRepo.GetByAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(result));
    }

    private void SetupRepositoryFailure()
    {
        Result<ApplicationUser, Error> result = new StubNotFoundError();
        _mockRepo.GetByAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(result));
    }

    private sealed class StubNotFoundError() : Error(ErrorType.NotFound, "User not found.");
}
