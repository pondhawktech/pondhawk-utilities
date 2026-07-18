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

public static class GetWidgetById
{
    public record Query(string Uid) : IQuery<Response>;

    public record Response(string Uid, string Name, string? Description, DateTime CreatedAt, DateTime? UpdatedAt);

    public class Handler(IDbContext db) : IQueryHandler<Query, Response>
    {
        public async Task<Response> HandleAsync(Query query, CancellationToken cancellationToken = default)
        {
            // Project entity -> response inside the query: the entity never leaves the handler.
            var response = await db.Set<Widget>()
                .Where(w => w.Uid == query.Uid)
                .Select(w => new Response(w.Uid, w.Name, w.Description, w.CreatedAt, w.UpdatedAt))
                .FirstOrDefaultAsync(cancellationToken);

            // Not-found is a failure, not a null success: the envelope maps it to 404 for us.
            return response ?? throw new NotFoundException("Widget", query.Uid);
        }
    }

    public class Endpoint : IEndpointModule
    {
        public string BasePath => "/widgets";

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/{uid}", ([AsParameters] Dispatcher dispatcher) => dispatcher.Send())
                .WithTags("Widgets")
                .WithName("GetWidgetById")
                .WithSummary("Get a widget by its UID")
                .Produces<Response>()
                .ProducesProblem(StatusCodes.Status404NotFound);
        }

        public sealed class Dispatcher : EndpointDispatcher
        {
            [FromRoute]
            public required string Uid { get; init; }

            public Task<IResult> Send()
            {
                var query = new Query(Uid);
                var result = Ok(query);
                return result;
            }
        }
    }
}
