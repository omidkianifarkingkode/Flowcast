using Shared.Application.Messaging;

namespace ZP.Core.Commands;

public sealed record HandlePaymentCallbackCommand(string Authority, string Status) : ICommand<CallbackHandleResult>;

public sealed record CallbackHandleResult(bool Success, string OrderId, long? RefId);
