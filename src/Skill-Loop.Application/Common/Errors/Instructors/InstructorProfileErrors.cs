using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Common.Errors.Instructors;

public static class InstructorProfileErrors
{
    public static readonly Error NotFound = new(
        "INSTRUCTOR_PROFILE_NOT_FOUND",
        "لم يتم العثور على الملف الشخصي للمدرب.",
        ErrorType.NotFound);

    public static readonly Error AlreadyExists = new(
        "INSTRUCTOR_PROFILE_ALREADY_EXISTS",
        "يوجد ملف شخصي لهذا المدرب بالفعل.",
        ErrorType.Conflict);
}