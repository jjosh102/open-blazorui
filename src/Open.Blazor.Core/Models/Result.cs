namespace Open.Blazor.Core.Models;

public class Result
{
    public Result(bool isSuccess, Error error)
    {
        if ((isSuccess && error != Error.None) || (!isSuccess && error == Error.None))
            throw new ArgumentException("Invalid error", nameof(error));
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public Error Error { get; }
    public bool IsFailure => !IsSuccess;

    public static Result Failure(Error error)
    {
        return new Result(false, error);
    }


    public static Result<T> Failure<T>(Error error)
    {
        return new Result<T>(default, false, error);
    }

    public static Result Success()
    {
        return new Result(true, Error.None);
    }


    public static Result<T> Success<T>(T value)
    {
        return new Result<T>(value, true, Error.None);
    }
}

public class Result<TValue>(TValue? value, bool isSuccess, Error error) : Result(isSuccess, error)
{
    public TValue? Value { get; } = value;

    public static implicit operator Result<TValue>(TValue value)
    {
        return new Result<TValue>(value, true, Error.None);
    }

    public static implicit operator Result<TValue>(Error error)
    {
        return new Result<TValue>(default, false, error);
    }
}