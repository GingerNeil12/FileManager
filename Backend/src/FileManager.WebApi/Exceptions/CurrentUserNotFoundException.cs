namespace FileManager.WebApi.Exceptions;

public class CurrentUserNotFoundException(string externalProviderId)
    : Exception($"Current user token with EPI: {externalProviderId} not found in DB.")
{
    
}