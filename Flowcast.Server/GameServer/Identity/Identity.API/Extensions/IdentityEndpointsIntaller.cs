using Identity.API.Endpoints;
using Microsoft.AspNetCore.Builder;

namespace Identity.API.Extensions;

public static class IdentityEndpointsIntaller
{
    public static void MapIdentityV1(this WebApplication app)
    {
        DeviceSignInFeature.Map(app);
        GoogleSignInFeature.Map(app);
        LinkFeature.Map(app);
        RefreshFeature.Map(app);
        LogoutFeature.Map(app);
        GetProfileFeature.Map(app);
    }
}
