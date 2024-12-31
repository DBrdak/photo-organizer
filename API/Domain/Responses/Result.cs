using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace API.Domain.Responses;

public class Result
{
    protected internal Result(bool isSuccess, string error)
    {
        switch (isSuccess)
        {
            case true when !string.IsNullOrEmpty(error):
                throw new InvalidOperationException("Success result mustn't contain error data");
            case false when string.IsNullOrEmpty(error):
                throw new InvalidOperationException("Failed result must contain error data");
            default:
                IsSuccess = isSuccess;
                Error = error;
                break;
        }
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; } = string.Empty;

    public static Result Success() => new(true, string.Empty);
    public static Result Failure(string error) => new(false, error);
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, string.Empty);
    public static Result<TValue> Failure<TValue>(string error) => new(default, false, error);

    public static Result<TValue> Create<TValue>(TValue? value) =>
        value is not null ? Success(value) : Failure<TValue>("Value cannot be null");

    public static Result<TValue> Create<TValue>(TValue? value, string error) =>
        value is not null ? Success(value) : Failure<TValue>(error);

    public static Result<TValue?> CreateNullable<TValue>(TValue? value) => Success(value);

    public static implicit operator Result(string error) => Failure(error);

    public static Result Aggregate(IEnumerable<Result> results)
    {
        var failedResult = results.FirstOrDefault(x => x.IsFailure);
        return failedResult ?? Success();
    }
}

public class Result<TValue> : Result
{
    private readonly TValue? _value;

    [JsonConstructor]
    [System.Text.Json.Serialization.JsonConstructor]
    public Result(TValue? value, bool isSuccess, string error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    [NotNullIfNotNull(nameof(_value))]
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException($"The value of a failure result cannot be accessed. Error: {Error}");

    public static implicit operator Result<TValue>(TValue? value) => Create(value);
    public static implicit operator Result<TValue>(string error) => Failure<TValue>(error);

    public TValue GetValueOrDefault(TValue defaultValue) =>
        IsSuccess ? Value : defaultValue;

    public Result<TNew> Map<TNew>(Func<TValue, TNew> mapper) =>
        IsSuccess ? Success(mapper(Value)) : Failure<TNew>(Error);

    public async Task<Result<TNew>> MapAsync<TNew>(Func<TValue, Task<TNew>> mapper) =>
        IsSuccess ? Success(await mapper(Value)) : Failure<TNew>(Error);

    public Result<TNew> Bind<TNew>(Func<TValue, Result<TNew>> binder) =>
        IsSuccess ? binder(Value) : Failure<TNew>(Error);

    public async Task<Result<TNew>> BindAsync<TNew>(Func<TValue, Task<Result<TNew>>> binder) =>
        IsSuccess ? await binder(Value) : Failure<TNew>(Error);
}