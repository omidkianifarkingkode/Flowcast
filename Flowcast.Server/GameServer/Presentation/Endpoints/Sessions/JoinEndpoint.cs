using Application.Abstractions.Messaging;
using Application.Sessions.Commands;
using Contracts.V1.Sessions;
using Presentation.Infrastructure;
using Presentation.Mappings;
using SharedKernel;

namespace Presentation.Endpoints.Sessions;

internal sealed class JoinEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(Join.Route,
            async (Join.Request request, ICommandHandler<JoinSessionCommand, JoinSessionResult> handler, CancellationToken ct) =>
        {
            request.MapToCommand(out JoinSessionCommand command);

            var result = await handler.Handle(command, ct);

            return result.Match(
                result =>
                {
                    result.MapToResponse(out Join.Response response);
                    return Results.Ok(response);
                },
                CustomResults.Problem
            );
        })
        .WithTags(Tags.Sessions)
        .MapToApiVersion(1.0);
    }
}
