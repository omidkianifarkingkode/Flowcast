using Microsoft.AspNetCore.Routing;

namespace Shared.API.Endpoints;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}