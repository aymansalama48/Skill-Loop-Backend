namespace Skill_Loop.Application.Features.Support.Share;

/// <summary>
/// بادئات مفاتيح الكاش الخاصة بميزة الدعم الفني.
/// المسح يتم بالبادئة (RemoveByPrefixAsync) فيكفي تمرير "support:questions" لمسح كل الصفحات والبحثات.
/// </summary>
public static class SupportCacheKeys
{
    /// <summary>كل استعلامات قائمة الأسئلة (الصفحات + البحث + التصنيف).</summary>
    public const string Questions = "support:questions";

    /// <summary>استعلامات الأسئلة الليستة للشخص نفسه.</summary>
    public const string MyQuestions = "support:my-questions:";

    /// <summary>استعلام سؤال واحد بالـ Id.</summary>
    public const string QuestionById = "support:question:";

    /// <summary>كل البادئات اللي لازم تتمسح بعد أي تعديل على الأسئلة.</summary>
    public static readonly string[] All = [Questions, MyQuestions, QuestionById];
}
