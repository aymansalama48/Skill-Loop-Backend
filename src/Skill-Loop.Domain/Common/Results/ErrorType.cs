namespace Skill_Loop.Domain.Common.Results;

public enum ErrorType
{
    Failure = 0,         // خطأ عام في منطق العمل (Business Logic)
    Validation = 1,      // خطأ في مدخلات الحقول (مثل حقل فارغ أو صيغة إيميل خاطئة)
    NotFound = 2,        // عنصر غير موجود في قاعدة البيانات
    Unauthorized = 3,    // غير مسجل دخول أو بيانات الدخول خاطئة
    Forbidden = 4,       // مسجل دخول ولكن لا يملك صلاحية (Role/Permission) لهذه العملية
    Conflict = 5,        // تضارب بيانات (مثل تكرار الفريد كالإيميل أو كود المشروع)
    Unexpected = 6       // خطأ غير متوقع أو كارثة تقنية (Database Crash / NullReference)
}