using System.Security.Claims;

using FileManager.Application.Common.Interfaces;
using FileManager.Domain.Common;
using FileManager.Domain.Common.Enums;
using FileManager.Domain.Common.Errors;
using FileManager.Domain.Models;
using FileManager.WebApi.Exceptions;
using FileManager.WebApi.Services;

using Microsoft.AspNetCore.Http;

using NSubstitute;

namespace FileManager.WebApi.Tests.Services;

[TestFixture]
public class CurrentUserTests
{
    private IHttpContextAccessor _mockAccessor;
    private IApplicationUserRepository _mockRepo;

    private const string ClaimsPrefix = "http://localhost/";
    private const string EmailClaimType = $"{ClaimsPrefix}email";
    private const string NameClaimType = $"{ClaimsPrefix}name";
    private const string RolesClaimType = $"{ClaimsPrefix}roles";

    [SetUp]
    public void SetUp()
    {
        _mockAccessor = Substitute.For<IHttpContextAccessor>();
        _mockRepo = Substitute.For<IApplicationUserRepository>();
    }

    [Test]
    public void Constructor_WhenAllClaimsValidAndUserFound_SetsAllProperties()
    {
        // Arrange
        ApplicationUser user = ApplicationUser.Create("auth0|12345");
        SetupHttpContext(BuildValidClaims());
        SetupRepositorySuccess(user);

        // Act
        CurrentUser sut = new(_mockAccessor, _mockRepo);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(sut.GetEmail(), Is.EqualTo("test@example.com"));
            Assert.That(sut.GetName(), Is.EqualTo("Test User"));
            Assert.That(sut.GetExternalProviderId(), Is.EqualTo("auth0|12345"));
            Assert.That(sut.GetRole(), Is.EqualTo(UserRoles.InternalAdmin));
            Assert.That(sut.GetUserId(), Is.EqualTo(user.Id));
        }
    }

    [TestCaseSource(nameof(MissingClaimCases))]
    public void Constructor_WhenRequiredClaimMissing_Throws(string missingClaimType)
    {
        // Arrange
        List<Claim> claims = BuildValidClaims()
            .Where(c => c.Type != missingClaimType)
            .ToList();
        SetupHttpContext(claims);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => new CurrentUser(_mockAccessor, _mockRepo));
    }

    [Test]
    public void Constructor_WhenRoleValueInvalid_ThrowsArgumentException()
    {
        // Arrange
        List<Claim> claims = BuildValidClaims()
            .Where(c => c.Type != RolesClaimType)
            .Append(new Claim(RolesClaimType, "NotAValidRole"))
            .ToList();
        SetupHttpContext(claims);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new CurrentUser(_mockAccessor, _mockRepo));
    }

    [Test]
    public void Constructor_WhenUserNotFoundInDb_ThrowsCurrentUserNotFoundException()
    {
        // Arrange
        SetupHttpContext(BuildValidClaims());
        SetupRepositoryFailure();

        // Act & Assert
        Assert.Throws<CurrentUserNotFoundException>(() => new CurrentUser(_mockAccessor, _mockRepo));
    }

    [TestCase(UserRoles.InternalAdmin, true)]
    [TestCase(UserRoles.InternalUser, false)]
    public void IsInRole_WhenChecked_ReturnsExpected(UserRoles roleToCheck, bool expected)
    {
        // Arrange
        ApplicationUser user = ApplicationUser.Create("auth0|12345");
        SetupHttpContext(BuildValidClaims());
        SetupRepositorySuccess(user);
        CurrentUser sut = new(_mockAccessor, _mockRepo);

        // Act
        bool result = sut.IsInRole(roleToCheck);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    private static IEnumerable<TestCaseData> MissingClaimCases()
    {
        yield return new TestCaseData(EmailClaimType)
            .SetName("Constructor_WhenEmailClaimMissing_Throws");
        yield return new TestCaseData(NameClaimType)
            .SetName("Constructor_WhenNameClaimMissing_Throws");
        yield return new TestCaseData(ClaimTypes.NameIdentifier)
            .SetName("Constructor_WhenNameIdentifierClaimMissing_Throws");
        yield return new TestCaseData(RolesClaimType)
            .SetName("Constructor_WhenRolesClaimMissing_Throws");
    }

    private static List<Claim> BuildValidClaims() =>
    [
        new Claim(EmailClaimType, "test@example.com"),
        new Claim(NameClaimType, "Test User"),
        new Claim(ClaimTypes.NameIdentifier, "auth0|12345"),
        new Claim(RolesClaimType, nameof(UserRoles.InternalAdmin))
    ];

    private void SetupHttpContext(IEnumerable<Claim> claims)
    {
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        _mockAccessor.HttpContext.Returns(httpContext);
    }

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
