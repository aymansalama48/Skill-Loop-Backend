using System.Text.Json.Serialization;

namespace Skill_Loop.Domain.Common.Results;

// ==========================================================
// يمثل نتيجة أي عملية داخل النظام (Domain & Application Layers)
// ==========================================================
public class Result
{
    public bool Succeeded { get; }

    [JsonIgnore]
    public bool IsSuccess => Succeeded;

    [JsonIgnore]
    public bool IsFailure => !Succeeded;

    public string? Message { get; }

    public IReadOnlyList<Error> Errors { get; }

    [JsonConstructor] // 👈 التعديل هنا: استخدام IReadOnlyList ليتطابق مع الخاصية
    protected Result(
        bool succeeded,
        string? message,
        IReadOnlyList<Error>? errors)
    {
        Succeeded = succeeded;
        Message = message;
        Errors = errors ?? new List<Error>();
    }

    public static Result Success(string? message = null)
    {
        return new Result(true, message, null);
    }

    public static Result Failure(Error error)
    {
        return new Result(false, null, new[] { error });
    }

    public static Result Failure(IEnumerable<Error> errors)
    {
        return new Result(false, null, errors.ToList());
    }

    public static Result Failure(string error)
    {
        return Failure(new Error(string.Empty, error, ErrorType.Failure));
    }

    public static Result Failure(IEnumerable<string> errors)
    {
        return Failure(errors.Select(e => new Error(string.Empty, e, ErrorType.Failure)));
    }
}

// ==========================================================
// نتيجة عملية مع بيانات
// ==========================================================
public sealed class Result<T> : Result
{
    public T? Data { get; }

    [JsonConstructor] // 👈 التعديل هنا أيضاً: استخدام IReadOnlyList
    private Result(
        bool succeeded,
        T? data,
        string? message,
        IReadOnlyList<Error>? errors) : base(succeeded, message, errors)
    {
        Data = data;
    }

    public static Result<T> Success(T data, string? message = null)
    {
        return new Result<T>(true, data, message, null);
    }

    public static new Result<T> Failure(Error error)
    {
        return new Result<T>(false, default, null, new[] { error });
    }

    public static new Result<T> Failure(IEnumerable<Error> errors)
    {
        return new Result<T>(false, default, null, errors.ToList());
    }

    public static new Result<T> Failure(string error)
    {
        return Failure(new Error(string.Empty, error, ErrorType.Failure));
    }

    public static new Result<T> Failure(IEnumerable<string> errors)
    {
        return Failure(errors.Select(e => new Error(string.Empty, e, ErrorType.Failure)));
    }
}