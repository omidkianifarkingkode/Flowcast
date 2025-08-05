using Application.Abstractions.Messaging;
using Application.Sessions.Queries;
using Contracts.V1.Sessions;
using Domain.Sessions;
using Domain.Sessions.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Presentation.Infrastructure;
using Presentation.Mappings;
using SharedKernel;

namespace Presentation.Endpoints.Sessions;

internal sealed class GetByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(Get.Route,
            async ([FromRoute] Guid sessionId, IQueryHandler<GetSessionByIdQuery, Session> handler, CancellationToken ct) =>
        {
            sessionId.MapToQuery(out GetSessionByIdQuery query);

            var result = await handler.Handle(query, ct);

            return result.Match(
                result =>
                {
                    result.MapToResponse(out Get.Response response);
                    return Results.Ok(response);
                },
                CustomResults.Problem
            );
        })
        .WithTags(Tags.Sessions)
        .MapToApiVersion(1.0);
    }
}
