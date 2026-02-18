using SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Shared.Application.Authentication;
using Shared.Application.Services;

namespace Shared.Infrastructure.Database;

public sealed class AuditInterceptor(IUserContext userContext, IDateTimeProvider dateTimeProvider) : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyAuditRules(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        // Optional: log or react after save
        return base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyAuditRules(DbContext? context)
    {
        if (context is null) return;

        var now = dateTimeProvider.UtcNowOffset;

        foreach (var entry in context.ChangeTracker.Entries())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    if (entry.Entity is ICreatable creatable)
                    {
                        creatable.CreatedAtUtc = now;
                        creatable.CreatorUser = userContext.UserId;
                    }

                    break;

                case EntityState.Modified:
                    if (entry.Entity is IModifiable modifiable)
                    {
                        modifiable.ModifiedAtUtc = now;
                        modifiable.ModifierUser = userContext.UserId;

                        // Prevent accidental modification of Created fields
                        if (entry.Entity is ICreatable)
                        {
                            entry.Property(nameof(ICreatable.CreatedAtUtc)).IsModified = false;
                            entry.Property(nameof(ICreatable.CreatorUser)).IsModified = false;
                        }
                    }
                    break;
            }
        }
    }
}
