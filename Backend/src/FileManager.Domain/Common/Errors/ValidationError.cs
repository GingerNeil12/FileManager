namespace FileManager.Domain.Common.Errors;

public class ValidationError(IDictionary<string, string[]> errors)
    : Error(ErrorType.Validation, "One or more validation errors occured.")
{
    public IReadOnlyDictionary<string, string[]> Errors => errors.AsReadOnly();
}