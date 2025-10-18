using PlayerProgressStore.Application.Services;
using PlayerProgressStore.Domain;
using Shared.Application.Messaging;
using SharedKernel;

namespace PlayerProgressStore.Application.Queries;

/// <summary>
/// Application query modeled after Contracts.V1.LoadProfile.
/// </summary>
public sealed record LoadProfileQuery(
    string PlayerId,
    IReadOnlyCollection<string>? Namespaces
) : IQuery<PlayerNamespace[]>;

public sealed class LoadProfileQueryHandler(IPlayerNamespaceRepository repo)
        : IQueryHandler<LoadProfileQuery, PlayerNamespace[]>
{
    public async Task<Result<PlayerNamespace[]>> Handle(LoadProfileQuery query, CancellationToken cancellationToken)
    {
        var loaded = await repo.LoadAsync(query.PlayerId, query.Namespaces, cancellationToken);
        if (loaded.IsFailure)
            return Result.Failure<PlayerNamespace[]>(loaded.Error);

        return Result.Success(loaded.Value.ToArray());
    }
}