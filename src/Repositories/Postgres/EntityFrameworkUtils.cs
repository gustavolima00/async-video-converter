using Microsoft.EntityFrameworkCore.Storage;

namespace Repositories.Postgres;

public static class EntityFrameworkUtils
{
    public static IDbContextTransaction? TryBeginTransaction(this DatabaseContext context)
    {
        if (context.SupportTransaction)
        {
            return context.Database.BeginTransaction();
        }
        return null;
    }

    public static async Task TryCommitAsync(this IDbContextTransaction? transaction, CancellationToken cancellationToken = default)
    {
        if (transaction is not null)
        {
            await transaction.CommitAsync(cancellationToken);
        }
    }

    public static async Task TryRollbackAsync(this IDbContextTransaction? transaction, CancellationToken cancellationToken = default)
    {
        if (transaction is not null)
        {
            await transaction.RollbackAsync(cancellationToken);
        }
    }
}
