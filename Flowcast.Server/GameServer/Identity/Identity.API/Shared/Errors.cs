// /Identity/Identity.Domain/Errors/DomainErrors.cs
using SharedKernel;

namespace Identity.API.Shared;

public static class DomainErrors
{
    // Business rule: you cannot link the "Device" provider.
    public static readonly Error InvalidProvider =
        Error.Problem("Identity.InvalidProvider", "Cannot link 'Device' as a provider.");

    // Business rule: single non-device provider per account.
    public static readonly Error AlreadyLinked =
        Error.Conflict("Identity.AlreadyLinked", "Account already linked to a non-device provider.");

    // After link, device login is disabled for this account.
    public static readonly Error DeviceLoginDisabled =
        Error.Conflict("Auth.DeviceDisabled", "Device identity is disabled after linking.");

    // Guard: an identity is not owned by this account (authorization-ish).
    public static readonly Error IdentityNotOwnedByAccount =
        Error.Unauthorized("Identity.NotOwned", "Identity does not belong to this account.");

    // (Optional) when a lookup by (provider, subject) returns no row.
    public static readonly Error IdentityNotFound =
        Error.NotFound("Identity.NotFound", "Identity was not found.");
}
