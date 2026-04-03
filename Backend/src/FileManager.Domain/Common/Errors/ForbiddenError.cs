namespace FileManager.Domain.Common.Errors;

public class ForbiddenError()
    : Error(ErrorType.Forbidden, "Access forbidden.")
{
    
}