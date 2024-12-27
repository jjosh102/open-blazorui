namespace Open.Blazor.Core.Models;

public record Error
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);
    public static readonly Error NullValue = new("Error.NullValue", "Value provided was null", ErrorType.Failure);

    public Error(string code, string description, ErrorType errorType)
    {
        Code = code;
        Description = description;
        ErrorType = errorType;
    }

    public string Code { get; }

    public string Description { get; }
    public ErrorType ErrorType { get; }

    public static Error NotFound(string code, string description)
    {
        return new Error(code, description, ErrorType.NotFound);
    }

    public static Error NotFound(string description)
    {
        return new Error("Error.NotFound", description, ErrorType.NotFound);
    }

    public static Error Failure(string code, string description)
    {
        return new Error(code, description, ErrorType.Failure);
    }

    public static Error Failure(string description)
    {
        return new Error("Error.Failure", description, ErrorType.Failure);
    }

    public static Error Validation(string code, string description)
    {
        return new Error(code, description, ErrorType.Validation);
    }

    public static Error Validation(string description)
    {
        return new Error("Error.Validation", description, ErrorType.Failure);
    }

    public static Error Conflict(string code, string description)
    {
        return new Error(code, description, ErrorType.Conflict);
    }

    public static Error Conflict(string description)
    {
        return new Error("Error.Conflict", description, ErrorType.Conflict);
    }

    public static implicit operator Result(Error error)
    {
        return Result.Failure(error);
    }
}

public enum ErrorType
{
    Failure,
    Validation,
    NotFound,
    Conflict
}