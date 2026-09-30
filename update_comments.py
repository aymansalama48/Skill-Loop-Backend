import os
import re

mapping = {
    "AdminEmailsController": "إدارة البريد الإلكتروني للمسؤولين (إرسال ومتابعة)",
    "AuthController": "إدارة المصادقة وتسجيل الدخول للمستخدمين",
    "BookingsController": "إدارة حجوزات الجلسات (إنشاء، إلغاء، وتحديث)",
    "CategoriesController": "إدارة التصنيفات الخاصة بالكورسات والمجالات",
    "ChatController": "إدارة المحادثات والرسائل بين المستخدمين",
    "CourseLessonsController": "إدارة دروس الكورسات ومحتواها",
    "CourseMaterialsController": "إدارة المواد التعليمية والملحقات الخاصة بالكورسات",
    "CourseReviewsController": "إدارة التقييمات والمراجعات الخاصة بالكورسات",
    "CoursesController": "إدارة الكورسات (إنشاء، تحديث، ونشر)",
    "CourseSectionsController": "إدارة أقسام الكورسات التعليمية",
    "CreditPurchasesController": "إدارة عمليات شراء الرصيد (Credits) وإضافتها للمحفظة",
    "DevController": "أدوات وبيانات تجريبية للمطورين (لبيئة التطوير فقط)",
    "EnrollmentsController": "إدارة اشتراكات الطلاب في الكورسات",
    "InstructorAvailabilitiesController": "إدارة أوقات الفراغ والمواعيد المتاحة للمحاضرين",
    "InstructorProfilesController": "إدارة الملفات الشخصية للمحاضرين وبياناتهم",
    "InstructorReviewsController": "إدارة التقييمات الخاصة بالمحاضرين",
    "NotificationsController": "إدارة الإشعارات والتنبيهات للمستخدمين",
    "OtpController": "إدارة رموز التحقق (OTP) للبريد الإلكتروني والهاتف",
    "PasswordsController": "إدارة إعادة تعيين واستعادة كلمات المرور",
    "PermissionManagementController": "إدارة الصلاحيات والأدوار (Roles) في النظام",
    "ProfileController": "إدارة الملف الشخصي للمستخدم الحالي",
    "PromoCodesController": "إدارة كوبونات الخصم والرموز الترويجية",
    "SessionBookingsController": "إدارة الحجوزات المرتبطة بجلسة معينة",
    "SessionMaterialsController": "إدارة الملحقات والمواد الخاصة بالجلسات",
    "SessionReviewsController": "إدارة التقييمات والمراجعات الخاصة بالجلسات",
    "SessionsController": "إدارة الجلسات المباشرة (الجدولة والنشر)",
    "SiteSettingsController": "إدارة إعدادات الموقع العامة والتكوينات",
    "StaffInvitationsController": "إدارة دعوات انضمام فريق العمل (Staff/Admins)",
    "SupportController": "إدارة طلبات الدعم الفني من المستخدمين",
    "SupportManagementController": "إدارة التذاكر وطلبات الدعم من قِبل الإدارة",
    "UsersController": "إدارة مستخدمي النظام بشكل عام",
    "WalletsController": "إدارة المحافظ المالية والرصيد",
}

directory = r"src\Skill-Loop.Api\Controllers"

for filename in os.listdir(directory):
    if filename.endswith(".cs"):
        filepath = os.path.join(directory, filename)
        class_name = filename[:-3]
        if class_name in mapping:
            with open(filepath, 'r', encoding='utf-8') as f:
                content = f.read()
            
            # Remove existing summary block completely
            content = re.sub(r'/// <summary>\s*/// .*?\s*/// </summary>\s*', '', content, flags=re.DOTALL)
            # Remove any stray summary lines
            content = re.sub(r'/// </?summary>.*?\n', '', content)
            
            summary = f"/// <summary>\n/// {mapping[class_name]}\n/// </summary>\n"
            
            # Insert before [Route or [ApiController or [Authorize or public class
            content = re.sub(r'^(\s*)(?=\[Route|\[ApiController|\[Authorize|\[Tags|public class)', r'\1' + summary, content, count=1, flags=re.MULTILINE)
            
            with open(filepath, 'w', encoding='utf-8') as f:
                f.write(content)
