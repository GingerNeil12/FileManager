namespace FileManager.WebApi.Option;

public class CorsOptions
{
    public const string SectionName = "Cors";
    public const string PolicyName = "AllowConfiguredOrigins";

    public string[] AllowedOrigins { get; set; } = [];
}
