using Skill_Loop.Application.Common.Abstractions.Persistence.Transaction;
using Skill_Loop.Infrastructure.Persistence.Data;

namespace Skill_Loop.Infrastructure.Persistence.Transaction;

using Microsoft.EntityFrameworkCore;
public sealed class EfTransactionManager : ITransactionManager
{
    private readonly AppDbContext _context;

    public EfTransactionManager(AppDbContext context)
    {
        _context = context;
    }

    public async Task ExecuteAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        var strategy = _context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(_context, async (context, ct) =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync(ct);

            await operation(ct);

            await context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }, cancellationToken);
    }

    public async Task<TResult> ExecuteAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default)
    {
        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(_context, async (context, ct) =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync(ct);

            var result = await operation(ct);

            await context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return result;
        }, cancellationToken);
    }
}