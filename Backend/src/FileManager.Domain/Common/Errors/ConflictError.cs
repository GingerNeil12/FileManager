namespace FileManager.Domain.Common.Errors;

public class ConflictError()
    : Error(ErrorType.Conflict, "Conflict occured when attempting to update.")
{
    
}