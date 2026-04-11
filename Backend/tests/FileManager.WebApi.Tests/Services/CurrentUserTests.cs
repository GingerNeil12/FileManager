using System.Security.Claims;

using FileManager.Application.Common.Interfaces;
using FileManager.Domain.Common.Enums;
using FileManager.WebApi.Services;

using Microsoft.AspNetCore.Http;

using NSubstitute;

namespace FileManager.WebApi.Tests.Services;

[TestFixture]
public class CurrentUserTests
{
    private IHttpContextAccessor _mockAccessor;

    private const string CLAIMS_PREFIX = "http://localhost/";
    private const string EMAIL_CLAIM_TYPE = $"{CLAIMS_PREFIX}email";
    private const string NAME_CLAIM_TYPE = $"{CLAIMS_PREFIX}name";
    private const string ROLES_CLAIM_TYPE = $"{CLAIMS_PREFIX}roles";

    [SetUp]
    public void SetUp()
    {
        _mockAccessor = Substitute.For<IHttpContextAccessor>();
    }

    [Test]
    public void Constructor_WhenAllClaimsAndUserIdInContext_SetsAllProperties()
    {
        // Arrange
        Guid expectedUserId = Guid.NewGuid();
        SetupHttpContext(BuildValidClaims(), expectedUserId);

        // Act
        CurrentUser sut = new(_mockAccessor);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(sut.GetEmail(), Is.EqualTo("test@example.com"));
            Assert.That(sut.GetName(), Is.EqualTo("Test User"));
            Assert.That(sut.GetExternalProviderId(), Is.EqualTo("auth0|12345"));
            Assert.That(sut.GetRole(), Is.EqualTo(UserRoles.InternalAdmin));
            Assert.That(sut.GetUserId(), Is.EqualTo(expectedUserId));
        }
    }

    [TestCaseSource(nameof(MissingClaimCases))]
    public void Constructor_WhenRequiredClaimMissing_Throws(string missingClaimType)
    {
        // Arrange
        List<Claim> claims = BuildValidClaims()
            .Where(c => c.Type != missingClaimType)
            .ToList();
        SetupHttpContext(claims, Guid.NewGuid());

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => new CurrentUser(_mockAccessor));
    }

    [Test]
    public void Constructor_WhenRoleValueInvalid_ThrowsArgumentException()
    {
        // Arrange
        List<Claim> claims = BuildValidClaims()
            .Where(c => c.Type != ROLES_CLAIM_TYPE)
            .Append(new Claim(ROLES_CLAIM_TYPE, "NotAValidRole"))
            .ToList();
        SetupHttpContext(claims, Guid.NewGuid());

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new CurrentUser(_mockAccessor));
    }

    [TestCase(UserRoles.InternalAdmin, true)]
    [TestCase(UserRoles.InternalUser, false)]
    public void IsInRole_WhenChecked_ReturnsExpected(UserRoles roleToCheck, bool expected)
    {
        // Arrange
        SetupHttpContext(BuildValidClaims(), Guid.NewGuid());
        CurrentUser sut = new(_mockAccessor);

        // Act
        bool result = sut.IsInRole(roleToCheck);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    private static IEnumerable<TestCaseData> MissingClaimCases()
    {
        yield return new TestCaseData(EMAIL_CLAIM_TYPE)
            .SetName("Constructor_WhenEmailClaimMissing_Throws");
        yield return new TestCaseData(NAME_CLAIM_TYPE)
            .SetName("Constructor_WhenNameClaimMissing_Throws");
        yield return new TestCaseData(ClaimTypes.NameIdentifier)
            .SetName("Constructor_WhenNameIdentifierClaimMissing_Throws");
        yield return new TestCaseData(ROLES_CLAIM_TYPE)
            .SetName("Constructor_WhenRolesClaimMissing_Throws");
    }

    private static List<Claim> BuildValidClaims() =>
    [
        new Claim(EMAIL_CLAIM_TYPE, "test@example.com"),
        new Claim(NAME_CLAIM_TYPE, "Test User"),
        new Claim(ClaimTypes.NameIdentifier, "auth0|12345"),
        new Claim(ROLES_CLAIM_TYPE, nameof(UserRoles.InternalAdmin))
    ];

    private void SetupHttpContext(IEnumerable<Claim> claims, Guid userId)
    {
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        httpContext.Items[ApplicationConstants.CURRENT_USER_ID] = userId;
        _mockAccessor.HttpContext.Returns(httpContext);
    }
}
