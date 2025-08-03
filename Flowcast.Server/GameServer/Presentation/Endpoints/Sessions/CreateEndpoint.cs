using Application.Abstractions.Messaging;
using Application.Sessions.Commands;
using Application.Sessions.Queries;
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
        app.MapPost(Create.Route, async (
            Create.Request request,
            ICommandHandler<CreateSessionCommand, SessionId> handler,
            CancellationToken ct) =>
        {
            var command = request.ToCommand();

            var result = await handler.Handle(command, ct);

            return result.Match(
                sessionId => Results.Ok(sessionId.ToResponse()),
                CustomResults.Problem
            );
        })
        .WithTags(Tags.Sessions)
        .MapToApiVersion(1.0);
    }
}

internal sealed class EndEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(End.Route, async (
            Guid sessionId,
            ICommandHandler<EndSessionCommand> handler,
            CancellationToken ct) =>
        {
            var command = sessionId.ToCommand();

            var result = await handler.Handle(command, ct);

            return result.Match(
                () => Results.NoContent(),
                CustomResults.Problem
            );
        })
        .WithTags(Tags.Sessions)
        .MapToApiVersion(1.0);
    }
}

internal sealed class JoinEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(Join.Route, async (
            Guid sessionId,
            Join.Request request,
            ICommandHandler<JoinSessionCommand> handler,
            CancellationToken ct) =>
        {
            var command = new JoinSessionCommand(
                SessionId.FromGuid(sessionId),
                request.PlayerId,
                request.DisplayName
            );

            var result = await handler.Handle(command, ct);

            return result.Match(
                () => Results.NoContent(),
                CustomResults.Problem
            );
        })
        .WithTags(Tags.Sessions)
        .MapToApiVersion(1.0);
    }
}

internal sealed class LeaveEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(Leave.Route, async (
            Guid sessionId,
            Leave.Request request,
            ICommandHandler<LeaveSessionCommand> handler,
            CancellationToken ct) =>
        {
            var command = new LeaveSessionCommand(
                SessionId.FromGuid(sessionId),
                request.PlayerId
            );

            var result = await handler.Handle(command, ct);

            return result.Match(
                () => Results.NoContent(),
                CustomResults.Problem
            );
        })
        .WithTags(Tags.Sessions)
        .MapToApiVersion(1.0);
    }
}

internal sealed class PlayerReadyEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(Ready.Route, async (
            Guid sessionId,
            Ready.Request request,
            ICommandHandler<PlayerReadyCommand> handler,
            CancellationToken ct) =>
        {
            var command = new PlayerReadyCommand(
                SessionId.FromGuid(sessionId),
                request.PlayerId
            );

            var result = await handler.Handle(command, ct);

            return result.Match(
                () => Results.NoContent(),
                CustomResults.Problem
            );
        })
        .WithTags(Tags.Sessions)
        .MapToApiVersion(1.0);
    }
}

internal sealed class GetByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(Get.Route, async (
            Guid sessionId,
            IQueryHandler<GetSessionQuery, Session> handler,
            CancellationToken ct) =>
        {
            var query = new GetSessionQuery(SessionId.FromGuid(sessionId));

            var result = await handler.Handle(query, ct);

            return result.Match(
                session => Results.Ok(result.Value.ToGetResponse()),
                CustomResults.Problem
            );
        })
        .WithTags(Tags.Sessions)
        .MapToApiVersion(1.0);
    }
}

internal sealed class GetForPlayerEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(GetForPlayer.Route, async (
            long playerId,
            IQueryHandler<GetSessionsForPlayerQuery, List<Session>> handler,
            CancellationToken ct) =>
        {
            var query = new GetSessionsForPlayerQuery(playerId);

            var result = await handler.Handle(query, ct);

            return result.Match(
                sessions => Results.Ok(result.Value.First().ToGetForPlayerResponse()),
                CustomResults.Problem
            );
        })
        .WithTags(Tags.Sessions)
        .MapToApiVersion(1.0);
    }
}
