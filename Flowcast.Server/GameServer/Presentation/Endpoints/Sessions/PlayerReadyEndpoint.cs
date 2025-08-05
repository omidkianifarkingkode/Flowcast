using Application.Abstractions.Messaging;
using Application.Sessions.Commands;
using Contracts.V1.Sessions;
using Presentation.Infrastructure;
using Presentation.Mappings;
using SharedKernel;

namespace Presentation.Endpoints.Sessions;

internal sealed class PlayerReadyEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(Ready.Route,
            async (Ready.Request request, ICommandHandler<PlayerReadyCommand, PlayerReadyResult> handler, CancellationToken ct) =>
        {
            request.MapToCommand(out PlayerReadyCommand command);

            var result = await handler.Handle(command, ct);

            return result.Match(
                result =>
                {
                    result.MapToResponse(out Ready.Response response);
                    return Results.Ok(response);
                },
                CustomResults.Problem
            );
        })
        .WithTags(Tags.Sessions)
        .MapToApiVersion(1.0);
    }
}
