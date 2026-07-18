using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Pondhawk.Api.Endpoints;
using Pondhawk.Mediator;
using Pondhawk.Persistence;
using Pondhawk.Sample.Api.Domain;

namespace Pondhawk.Sample.Api.Features.Widgets;

// One file = one operation. The static wrapper class is the slice; the request, response, handler,
// and endpoint (with its nested Dispatcher) all live inside it. To add a resource, copy the file
// and rename the wrapper. Nothing is hidden in a base class.
public static class CreateWidget
{
    public record Command(string Name, string? Description) : ICommand<Response>;

    public record Response(string Uid, string Name, string? Description, DateTime CreatedAt);

    public class Handler(IDbContext db) : ICommandHandler<Command, Response>
    {
        public async Task<Response> HandleAsync(Command command, CancellationToken cancellationToken = default)
        {
            var widget = new Widget
            {
                Uid = Ulid.NewUlid().ToString(),
                Name = command.Name,
                Description = command.Description,
                CreatedAt = DateTime.UtcNow,
            };

            db.Set<Widget>().Add(widget);
            await db.SaveChangesAsync(cancellationToken);

            return new Response(widget.Uid, widget.Name, widget.Description, widget.CreatedAt);
        }
    }

    public class Endpoint : IEndpointModule
    {
        public string BasePath => "/widgets";

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/", ([AsParameters] Dispatcher dispatcher) => dispatcher.Send())
                .WithTags("Widgets")
                .WithName("CreateWidget")
                .WithSummary("Create a new widget")
                .Accepts<Command>("application/json")
                .Produces<Response>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status422UnprocessableEntity);
        }

        public sealed class Dispatcher : EndpointDispatcher
        {
            public required Command Command { get; init; }

            public Task<IResult> Send()
            {
                var result = Created(Command, created => $"/api/widgets/{created.Uid}");
                return result;
            }
        }
    }
}
