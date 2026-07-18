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

public static class DeleteWidget
{
    public record Command(string Uid) : ICommand<Receipt>;

    public class Handler(IDbContext db) : ICommandHandler<Command, Receipt>
    {
        public async Task<Receipt> HandleAsync(Command command, CancellationToken cancellationToken = default)
        {
            var widget = await db.Set<Widget>().FirstOrDefaultAsync(w => w.Uid == command.Uid, cancellationToken)
                ?? throw new NotFoundException("Widget", command.Uid);

            db.Set<Widget>().Remove(widget);
            await db.SaveChangesAsync(cancellationToken);

            return Receipt.One;
        }
    }

    public class Endpoint : IEndpointModule
    {
        public string BasePath => "/widgets";

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("/{uid}", ([AsParameters] Dispatcher dispatcher) => dispatcher.Send())
                .WithTags("Widgets")
                .WithName("DeleteWidget")
                .WithSummary("Delete a widget")
                .Produces(StatusCodes.Status204NoContent)
                .ProducesProblem(StatusCodes.Status404NotFound);
        }

        public sealed class Dispatcher : EndpointDispatcher
        {
            [FromRoute]
            public required string Uid { get; init; }

            public Task<IResult> Send()
            {
                var command = new Command(Uid);
                var result = NoContent(command);
                return result;
            }
        }
    }
}
