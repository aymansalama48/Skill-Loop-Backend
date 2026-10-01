using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Api.Controllers.Base;
using Skill_Loop.Domain.Constants;
using Skill_Loop.Infrastructure.Persistence.Data;

namespace Skill_Loop.Api.Controllers.System;

/// <summary>
/// متحكم لعرض سجلات النظام (متاح فقط للـ SuperAdmin)
/// </summary>
[Route("api/v1/system/audit-logs")]
[Authorize]
[Tags("System")]
public class AuditLogsController : BaseApiController
{
    private readonly AppDbContext _context;

    public AuditLogsController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// جلب سجلات التغييرات في النظام مع دعم التصفح
    /// </summary>
    [HttpGet]
    public async Task<IResult> GetAuditLogs(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs.AsNoTracking().OrderByDescending(x => x.Timestamp);
        
        var totalCount = await query.CountAsync(cancellationToken);
        
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new
            {
                a.Id,
                a.UserId,
                a.Action,
                a.EntityType,
                a.EntityId,
                a.Changes,
                a.Timestamp
            })
            .ToListAsync(cancellationToken);
            
        return Results.Ok(new
        {
            Data = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        });
    }
}
