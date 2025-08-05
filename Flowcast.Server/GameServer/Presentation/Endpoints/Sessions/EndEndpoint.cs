using Application.Abstractions.Messaging;
using Application.Sessions.Commands;
using Contracts.V1.Sessions;
using Domain.Sessions;
using Presentation.Infrastructure;
using Presentation.Mappings;
using SharedKernel;

namespace Presentation.Endpoints.Sessions;

internal sealed class EndEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(End.Route,
            async (End.Request request, ICommandHandler<EndSessionCommand, Session> handler, CancellationToken ct) =>
        {
            request.MapToCommand(out EndSessionCommand command);

            var result = await handler.Handle(command, ct);

            return result.Match(
                result =>
                {
                    result.MapToResponse(out End.Response response);
                    return Results.Ok(response);
                },
                CustomResults.Problem
            );
        })
        .WithTags(Tags.Sessions)
        .MapToApiVersion(1.0);
    }
}
