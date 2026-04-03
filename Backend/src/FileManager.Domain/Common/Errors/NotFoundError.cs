namespace FileManager.Domain.Common.Errors;

public class NotFoundError(Type objType, string id)
    : Error(ErrorType.NotFound, $"Model: {objType} with Id: {id} not found.")
{
    
}