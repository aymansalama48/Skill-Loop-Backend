namespace Skill_Loop.Domain.Common.Results;

// ==========================================================
// يمثل نتيجة أي عملية داخل النظام (Domain & Application Layers)
// ==========================================================
public class Result
{
    /// <summary>
    /// هل العملية نجحت؟
    /// </summary>
    public bool Succeeded { get; }

    /// <summary>
    /// خاصية مساعدة لسهولة القراءة (IsSuccess)
    /// </summary>
    public bool IsSuccess => Succeeded;

    /// <summary>
    /// خاصية مساعدة لسهولة القراءة (IsFailure)
    /// </summary>
    public bool IsFailure => !Succeeded;

    /// <summary>
    /// رسالة نجاح (اختيارية)
    /// </summary>
    public string? Message { get; }

    /// <summary>
    /// قائمة الأخطاء (ككائنات Error)
    /// </summary>
    public IReadOnlyList<Error> Errors { get; }

    protected Result(
        bool succeeded,
        string? message,
        IEnumerable<Error>? errors)
    {
        Succeeded = succeeded;
        Message = message;
        Errors = errors?.ToList() ?? [];
    }

    /// <summary>
    /// نجاح بدون بيانات
    /// </summary>
    public static Result Success(string? message = null)
    {
        return new Result(true, message, null);
    }

    /// <summary>
    /// فشل مع خطأ واحد (كـ Error)
    /// </summary>
    public static Result Failure(Error error)
    {
        return new Result(false, null, new[] { error });
    }

    /// <summary>
    /// فشل مع عدة أخطاء (كـ Error)
    /// </summary>
    public static Result Failure(IEnumerable<Error> errors)
    {
        return new Result(false, null, errors);
    }

    /// <summary>
    /// فشل برسالة نصية واحدة (يتم تحويلها إلى Error مع نوع Failure)
    /// </summary>
    public static Result Failure(string error)
    {
        return Failure(new Error(string.Empty, error, ErrorType.Failure));
    }

    /// <summary>
    /// فشل بعدة رسائل نصية (يتم تحويلها إلى Error مع نوع Failure)
    /// </summary>
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
    /// <summary>
    /// البيانات المرجعة
    /// </summary>
    public T? Data { get; }

    private Result(bool succeeded, T? data, string? message, IEnumerable<Error>? errors) : base(succeeded, message, errors)
    {
        Data = data;
    }

    /// <summary>
    /// نجاح مع بيانات
    /// </summary>
    public static Result<T> Success(T data, string? message = null)
    {
        return new Result<T>(true, data, message, null);
    }

    /// <summary>
    /// فشل مع خطأ واحد (كـ Error)
    /// </summary>
    public static new Result<T> Failure(Error error)
    {
        return new Result<T>(false, default, null, new[] { error });
    }

    /// <summary>
    /// فشل مع عدة أخطاء (كـ Error)
    /// </summary>
    public static new Result<T> Failure(IEnumerable<Error> errors)
    {
        return new Result<T>(false, default, null, errors);
    }

    /// <summary>
    /// فشل برسالة نصية واحدة (يتم تحويلها إلى Error مع نوع Failure)
    /// </summary>
    public static new Result<T> Failure(string error)
    {
        return Failure(new Error(string.Empty, error, ErrorType.Failure));
    }

    /// <summary>
    /// فشل بعدة رسائل نصية (يتم تحويلها إلى Error مع نوع Failure)
    /// </summary>
    public static new Result<T> Failure(IEnumerable<string> errors)
    {
        return Failure(errors.Select(e => new Error(string.Empty, e, ErrorType.Failure)));
    }
}