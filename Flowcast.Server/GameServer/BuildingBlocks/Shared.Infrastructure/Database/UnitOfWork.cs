namespace Shared.Infrastructure.Database;

using Microsoft.EntityFrameworkCore;
using Shared.Application.Services;
using SharedKernel;
using System;
using System.Threading;
using System.Threading.Tasks;

public class UnitOfWork<TContext>(TContext context) : IUnitOfWork where TContext : DbContext
{
    public async Task<Result> ExecuteAsync(Func<CancellationToken, Task<Result>> action, CancellationToken ct = default)
    {
        using var transaction = await context.Database.BeginTransactionAsync(ct);
        try
        {
            var result = await action(ct);
            if (result.IsFailure)
            {
                await transaction.RollbackAsync(ct);
                return result;
            }

            await context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return Result.Success();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(ct);
            return Result.Failure(Error.Failure("database.save_error", ex.Message));
        }
    }

    public async Task<Result<T>> ExecuteAsync<T>(Func<CancellationToken, Task<Result<T>>> action, CancellationToken ct = default)
    {
        using var transaction = await context.Database.BeginTransactionAsync(ct);
        try
        {
            var result = await action(ct);
            if (result.IsFailure)
            {
                await transaction.RollbackAsync(ct);
                return Result.Failure<T>(result.Error);
            }

            await context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return Result.Success(result.Value);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(ct);
            return Result.Failure<T>(Error.Failure("database.save_error", ex.Message));
        }
    }
}
