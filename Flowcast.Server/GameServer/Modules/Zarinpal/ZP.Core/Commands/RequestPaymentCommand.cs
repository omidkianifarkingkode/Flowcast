using Shared.Application.Messaging;
using ZP.Contracts;

namespace ZP.Core.Commands;

public sealed record RequestPaymentCommand(
    string UserId,
    string ProductId,
    int Amount,
    string Description,
    string? Mobile = null,
    string? Email = null,
    string? IdempotencyKey = null) : ICommand<RequestPaymentCommandResult>;

public sealed record RequestPaymentCommandResult(RequestPaymentResponse Response, bool Created);
