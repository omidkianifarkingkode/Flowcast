using Microsoft.AspNetCore.Routing;

namespace Shared.Presentation.Endpoints;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}