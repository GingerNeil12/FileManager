namespace FileManager.Domain.Common;

public readonly struct Result<TValue,TError>
{
    private readonly TValue? _value;
    private readonly TError? _error;

    private Result(TValue value)
    {
        _value = value;
        _error = default;
    }

    private Result(TError error)
    {
        _error = error;
        _value = default;
    }

    public TValue? Value => _value;
    public TError? Error => _error;
    public bool IsSuccess => _error is not null;

    public static implicit operator Result<TValue,TError>(TValue value) => new(value);
    public static implicit operator Result<TValue,TError>(TError error) => new(error);
}