namespace FileManager.Domain.Common.Errors;

public class ExternalServiceError(string message) : Error(ErrorType.ExternalService, message)
{
}
