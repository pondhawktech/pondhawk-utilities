using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Pondhawk.Api.Endpoints;
using Pondhawk.Exceptions;
using Pondhawk.Mediator;
using Pondhawk.Persistence;
using Pondhawk.Sample.Api.Domain;

namespace Pondhawk.Sample.Api.Features.Widgets;

public static class UpdateWidget
{
    public record Command(string Uid, string Name, string? Description) : ICommand<Response>;

    public record Response(string Uid, string Name, string? Description, DateTime? UpdatedAt);

    // The request body: the UID travels in the route, so it is not part of the posted body.
    public record Body(string Name, string? Description);

    public class Handler(IDbContext db) : ICommandHandler<Command, Response>
    {
        public async Task<Response> HandleAsync(Command command, CancellationToken cancellationToken = default)
        {
            var widget = await db.Set<Widget>().FirstOrDefaultAsync(w => w.Uid == command.Uid, cancellationToken)
                ?? throw new NotFoundException("Widget", command.Uid);

            widget.Name = command.Name;
            widget.Description = command.Description;
            widget.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync(cancellationToken);

            return new Response(widget.Uid, widget.Name, widget.Description, widget.UpdatedAt);
        }
    }

    public class Endpoint : IEndpointModule
    {
        public string BasePath => "/widgets";

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/{uid}", ([AsParameters] Dispatcher dispatcher) => dispatcher.Send())
                .WithTags("Widgets")
                .WithName("UpdateWidget")
                .WithSummary("Update an existing widget")
                .Accepts<Body>("application/json")
                .Produces<Response>()
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status422UnprocessableEntity);
        }

        public sealed class Dispatcher : EndpointDispatcher
        {
            [FromRoute]
            public required string Uid { get; init; }

            public required Body Body { get; init; }

            public Task<IResult> Send()
            {
                var command = new Command(Uid, Body.Name, Body.Description);
                var result = Ok(command);
                return result;
            }
        }
    }
}
