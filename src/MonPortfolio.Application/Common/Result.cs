namespace MonPortfolio.Application.Common;

/// <summary>
/// Issue d'un cas d'utilisation. Les erreurs attendues (validation, règle métier, introuvable)
/// sont retournées plutôt que levées, l'UI les affiche telles quelles.
/// </summary>
public class Result
{
    protected Result(bool isSuccess, IReadOnlyList<string> errors)
    {
        IsSuccess = isSuccess;
        Errors = errors;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public IReadOnlyList<string> Errors { get; }
    public string Error => string.Join(" ", Errors);

    public static Result Success() => new(true, []);

    public static Result Failure(params string[] errors) => new(false, errors);

    public static Result Failure(IEnumerable<string> errors) => new(false, errors.ToList());

    public static Result NotFound(string resource) => Failure($"{resource} introuvable.");
}

public sealed class Result<T> : Result
{
    private Result(T value) : base(true, []) => Value = value;

    private Result(IReadOnlyList<string> errors) : base(false, errors) { }

    public T? Value { get; }

    public static Result<T> Success(T value) => new(value);

    public static new Result<T> Failure(params string[] errors) => new(errors);

    public static new Result<T> Failure(IEnumerable<string> errors) => new(errors.ToList());

    public static implicit operator Result<T>(T value) => Success(value);
}
