using Application.Abstractions.Messaging;
using Application.Sessions.Queries;
using Contracts.V1.Sessions;
using Domain.Sessions;
using Microsoft.AspNetCore.Mvc;
using Presentation.Infrastructure;
using Presentation.Mappings;
using SharedKernel;

namespace Presentation.Endpoints.Sessions;

internal sealed class GetForPlayerEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(GetByPlayer.Route,
            async ([FromRoute] Guid playerId, IQueryHandler<GetSessionByPlayerQuery, Session> handler, CancellationToken ct) =>
        {
            playerId.MapToQuery(out GetSessionByPlayerQuery query);

            var result = await handler.Handle(query, ct);

            return result.Match(
                result =>
                {
                    result.MapToResponse(out GetByPlayer.Response response);
                    return Results.Ok(response);
                },
                CustomResults.Problem
            );
        })
        .WithTags(Tags.Sessions)
        .MapToApiVersion(1.0);
    }
}
