using Microsoft.AspNetCore.Http;
using SharedKernel;
using System.Security.Claims;

namespace Identity.API.Extensions;

internal static class HttpContextExtensions
{
    private const string AccountID = "aid";
    private const string Subject = "sub";

    public static Result<Guid> GetAccountId(this HttpContext http)
    {
        // Prefer "aid" or "sub" claim, fallback to NameIdentifier
        var str = http.User.FindFirst(AccountID)?.Value
                ?? http.User.FindFirst(Subject)?.Value
               ?? http.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(str))
            return Result.Failure<Guid>(Errors.MissingAccount);

        if (!Guid.TryParse(str, out var accountId))
            return Result.Failure<Guid>(Errors.InvalidAccount);

        return accountId;
    }

    public static class Errors 
    {
        public static Error MissingAccount = Error.Unauthorized("auth.missing_account_id",
                "Authenticated user does not contain an account id claim.");

        public static Error InvalidAccount = Error.Unauthorized("auth.invalid_account_id",
                "Account id claim is not a valid GUID.");
    }
}
