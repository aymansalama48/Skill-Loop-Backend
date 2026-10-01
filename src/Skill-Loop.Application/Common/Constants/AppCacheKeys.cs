namespace Skill_Loop.Application.Common.Constants;

public static class AppCacheKeys
{
    // Session Materials
    public static string SessionMaterials(Guid sessionId) => $"session-materials-{sessionId}";
    public static string SessionMaterialsAll => "session-materials-all";
    public static string SessionMaterial(Guid materialId) => $"session-material-{materialId}";
    public static string SessionMaterialDownload(Guid sessionId, Guid materialId) => $"session-material-download-{sessionId}-{materialId}";
    public static string SessionMaterialsPaged(Guid sessionId, int pageNumber, int pageSize) => $"session-materials-{sessionId}-page-{pageNumber}-size-{pageSize}";
    
    // Drive Quota
    public const string DriveQuotaUsage = "drive-quota-usage";
    
    // Users
    public const string UsersList = "users-list";

    // Courses
    public const string CoursesPrefix = "courses:";
    public const string CoursesPaged = "courses:paged:";
    public static string CourseById(Guid id) => $"courses:detail:{id}";
    public static string CoursesByCategory(Guid categoryId) => $"courses:category:{categoryId}";
    public static string CoursesByInstructor(Guid instructorId) => $"courses:instructor:{instructorId}";
    public static string CourseSections(Guid courseId) => $"courses:sections:{courseId}";
    public static string CourseLessons(Guid courseId) => $"courses:lessons:{courseId}";
    
    // Sessions
    public const string SessionsPrefix = "sessions:";
    public const string SessionsPaged = "sessions:paged:";
    public static string SessionById(Guid id) => $"sessions:detail:{id}";
    
    // Instructors
    public const string InstructorsPrefix = "instructors:";
    public const string InstructorsPaged = "instructors:paged:";
    public static string InstructorByUserId(Guid userId) => $"instructors:profile:{userId}";
    
    // Enrollments
    public const string EnrollmentsPrefix = "enrollments:";
    public static string UserEnrollments(Guid userId) => $"enrollments:user:{userId}";
    
    // Bookings
    public const string BookingsPrefix = "bookings:";
    public static string UserBookings(Guid userId) => $"bookings:user:{userId}";
    
    // Categories
    public const string CategoriesPrefix = "categories:";
}