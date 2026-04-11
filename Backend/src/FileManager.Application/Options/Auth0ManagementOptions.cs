namespace FileManager.Application.Options;

public class Auth0ManagementOptions
{
    public const string SECTION_NAME = "Auth0";

    public string Scheme { get; set; } = "https";
    public string Domain { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Connection { get; set; } = string.Empty;
}
