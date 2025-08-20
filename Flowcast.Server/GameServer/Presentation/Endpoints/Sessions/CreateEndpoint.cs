using Application.Abstractions.Messaging;
using Application.Sessions.Commands;
using Contracts.V1.Sessions;
using Domain.Sessions;
using Presentation.Infrastructure;
using Presentation.Mappings;
using SharedKernel;

namespace Presentation.Endpoints.Sessions;

internal sealed class CreateEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(Create.Route,
            async (Create.Request request, ICommandHandler<CreateSessionCommand, SessionId> handler, CancellationToken ct) =>
        {
            var command = request.MapToCommand();

            var result = await handler.Handle(command, ct);

            return result.Match(
                result =>
                {
                    result.MapToResponse(out Create.Response response);
                    return Results.Ok(response);
                },
                CustomResults.Problem
            );
        })
        .WithTags(Tags.Sessions)
        .MapToApiVersion(1.0);
    }
}