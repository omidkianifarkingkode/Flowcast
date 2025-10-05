using SharedKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerProgressStore.Application.Abstractions;

/// <summary>
/// Transaction boundary for atomic save.
/// Infra implementation should enlist the repository operations.
/// </summary>
public interface IUnitOfWork
{
    Task<Result> ExecuteAsync(Func<CancellationToken, Task<Result>> action, CancellationToken ct = default);
    Task<Result<T>> ExecuteAsync<T>(Func<CancellationToken, Task<Result<T>>> action, CancellationToken ct = default);
}
