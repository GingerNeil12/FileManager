namespace FileManager.Application.Options;

public class Auth0ManagementOptions
{
    public const string SectionName = "Auth0Management";

    public string Domain { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string ManagementAudience { get; set; } = string.Empty;
    public string Connection { get; set; } = string.Empty;
}
