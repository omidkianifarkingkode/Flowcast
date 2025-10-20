using Identity.Contracts.V1.Shared;

namespace Identity.Presentation.Endpoints.V1;

public static class IdentityProviderMapper 
{
    public static Domain.Shared.IdentityProvider MapToDomain(this Contracts.V1.Shared.IdentityProvider provider) 
    {
        return provider switch
        {
            IdentityProvider.Device => Domain.Shared.IdentityProvider.Device,
            IdentityProvider.Google => Domain.Shared.IdentityProvider.Google,
            IdentityProvider.Facebook => Domain.Shared.IdentityProvider.Facebook,
            IdentityProvider.Apple => Domain.Shared.IdentityProvider.Apple,
            _ => throw new NotImplementedException(),
        };
    }
}
