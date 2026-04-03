namespace FileManager.Domain.Common.Errors;

public abstract class Error(ErrorType type, string message)
{
    public ErrorType Type => type;
    public string Message => message;
}

public enum ErrorType
{
    Validation,
    NotFound,
    Forbidden,
    Conflict
}