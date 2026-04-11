namespace FileManager.WebApi.Exceptions
{
    public class UserBlockedException(string externalProviderId)
        : Exception($"Current user: {externalProviderId} is blocked.")
    {
        
    }
}