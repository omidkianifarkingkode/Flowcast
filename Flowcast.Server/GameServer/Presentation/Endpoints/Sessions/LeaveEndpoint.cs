using Application.Abstractions.Messaging;
using Application.Sessions.Commands;
using Contracts.V1.Sessions;
using Presentation.Infrastructure;
using Presentation.Mappings;
using SharedKernel;

namespace Presentation.Endpoints.Sessions;

internal sealed class LeaveEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(Leave.Route,
            async (Leave.Request request, ICommandHandler<LeaveSessionCommand, LeaveSessionResult> handler, CancellationToken ct) =>
        {
            request.MapToCommand(out LeaveSessionCommand command);

            var result = await handler.Handle(command, ct);

            return result.Match(
                result =>
                {
                    result.MapToResponse(out Leave.Response response);
                    return Results.Ok(response);
                },
                CustomResults.Problem
            );
        })
        .WithTags(Tags.Sessions)
        .MapToApiVersion(1.0);
    }
}