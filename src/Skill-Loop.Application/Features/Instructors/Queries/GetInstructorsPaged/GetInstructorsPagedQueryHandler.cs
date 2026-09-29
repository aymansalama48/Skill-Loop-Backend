using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Identity.UserManagement;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Pagination;
using Skill_Loop.Application.Features.Instructors.Share;
using Skill_Loop.Domain.Common.Results;

namespace Skill_Loop.Application.Features.Instructors.Queries.GetInstructorsPaged;

public sealed class GetInstructorsPagedQueryHandler(
    IApplicationDbContext _dbContext,
    IUserManagementService _userService) : IQueryHandler<GetInstructorsPagedQuery, PagedResult<InstructorSummaryResponse>>
{
    public async Task<Result<PagedResult<InstructorSummaryResponse>>> Handle(GetInstructorsPagedQuery request, CancellationToken cancellationToken)
    {
        // 1. نبدأ بالكويري الأساسي (المدربين المعتمدين فقط)
        var query = _dbContext.InstructorProfiles
            .AsNoTracking()
            .Where(p => p.IsApproved);

        // 2. تطبيق الفلترة (SearchTerm)
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.Trim().ToLower();
            query = query.Where(p =>
                p.Headline.ToLower().Contains(search) ||
                p.Bio.ToLower().Contains(search));
        }

        // 3. تطبيق الفلترة (الحد الأدنى للتقييم)
        if (request.MinRating.HasValue)
        {
            query = query.Where(p => p.Rating >= request.MinRating.Value);
        }

        // 4. تطبيق الفلترة (عنده خبرة/جلسات سابقة)
        if (request.HasCompletedSessions.HasValue && request.HasCompletedSessions.Value)
        {
            query = query.Where(p => p.SessionsCompleted > 0);
        }

        // 5. تطبيق الترتيب (Sorting)
        query = request.SortBy?.ToLower() switch
        {
            "sessions" => query.OrderByDescending(p => p.SessionsCompleted),
            "newest" => query.OrderByDescending(p => p.CreatedAt),
            "rating_asc" => query.OrderBy(p => p.Rating),
            _ => query.OrderByDescending(p => p.Rating) // الترتيب الافتراضي (الأعلى تقييماً)
        };

        // حساب العدد الإجمالي بعد الفلترة
        var totalCount = await query.CountAsync(cancellationToken);

        // 6. تطبيق الـ Pagination وجلب البيانات من الداتابيز
        var profiles = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        // 7. جلب بيانات المستخدمين (الاسم والصورة)
        var summaries = new List<InstructorSummaryResponse>();
        foreach (var profile in profiles)
        {
            var userResult = await _userService.GetByIdAsync(profile.UserId, cancellationToken);
            if (userResult.IsSuccess && userResult.Data is not null)
            {
                var user = userResult.Data;
                summaries.Add(new InstructorSummaryResponse(
                    profile.Id,
                    profile.UserId,
                    user.FullName,
                    user.AvatarUrl,
                    profile.Headline,
                    profile.Rating,
                    profile.SessionsCompleted
                ));
            }
        }

        // 8. تغليف النتيجة
        var pagedResult = new PagedResult<InstructorSummaryResponse>
        {
            Items = summaries,
            Pagination = new PaginationMetadata
            {
                TotalCount = totalCount,
                PageSize = request.PageSize,
                CurrentPage = request.PageNumber
            }
        };

        return Result<PagedResult<InstructorSummaryResponse>>.Success(pagedResult);
    }
}