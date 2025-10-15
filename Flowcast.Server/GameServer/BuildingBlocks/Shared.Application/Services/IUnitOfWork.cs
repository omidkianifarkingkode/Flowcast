using SharedKernel;

namespace Shared.Application.Services;

/// <summary>
/// Transaction boundary for atomic save.
/// Infra implementation should enlist the repository operations.
/// </summary>
public interface IUnitOfWork
{
    Task<Result> ExecuteAsync(Func<CancellationToken, Task<Result>> action, CancellationToken ct = default);
    Task<Result<T>> ExecuteAsync<T>(Func<CancellationToken, Task<Result<T>>> action, CancellationToken ct = default);
}
