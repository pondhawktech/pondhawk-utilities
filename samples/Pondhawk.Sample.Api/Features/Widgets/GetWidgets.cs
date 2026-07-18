using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Pondhawk.Api.Endpoints;
using Pondhawk.Mediator;
using Pondhawk.Persistence;
using Pondhawk.Rules;
using Pondhawk.Rules.Validators;
using Pondhawk.Sample.Api.Domain;

namespace Pondhawk.Sample.Api.Features.Widgets;

public static class GetWidgets
{
    public record Query(string? Name, int Page = 1, int PageSize = 20) : IQuery<PagedResponse<Response>>;

    public record Response(string Uid, string Name, string? Description, DateTime CreatedAt);

    // Request rule (API concern): input-shape validation on the Query, discovered by AddRules and run by
    // the mediator ValidationBehavior. Entity invariants live elsewhere (a Persistence concern).
    public class Rules : ValidationBuilder<Query>
    {
        public Rules()
        {
            Assert<int>(q => q.Page).IsGreaterThanOrEqual(1);
            Assert<int>(q => q.PageSize).IsGreaterThanOrEqual(1).IsLessThanOrEqual(100);
        }
    }

    public class Handler(IDbContext db) : IQueryHandler<Query, PagedResponse<Response>>
    {
        public async Task<PagedResponse<Response>> HandleAsync(Query query, CancellationToken cancellationToken = default)
        {
            var widgets = db.Set<Widget>().AsQueryable();

            var name = query.Name;
            if (!string.IsNullOrWhiteSpace(name))
                widgets = widgets.Where(w => w.Name.Contains(name));

            var total = await widgets.CountAsync(cancellationToken);

            var items = await widgets
                .OrderBy(w => w.Name)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(w => new Response(w.Uid, w.Name, w.Description, w.CreatedAt))
                .ToListAsync(cancellationToken);

            return new PagedResponse<Response>(items, query.Page, query.PageSize, total);
        }
    }

    public class Endpoint : IEndpointModule
    {
        public string BasePath => "/widgets";

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/", ([AsParameters] Dispatcher dispatcher) => dispatcher.Send())
                .WithTags("Widgets")
                .WithName("GetWidgets")
                .WithSummary("List widgets (paged, optional name filter)")
                .Produces<PagedResponse<Response>>();
        }

        public sealed class Dispatcher : EndpointDispatcher
        {
            [FromQuery]
            public string? Name { get; init; }

            [FromQuery]
            public int? Page { get; init; }

            [FromQuery]
            public int? PageSize { get; init; }

            public Task<IResult> Send()
            {
                var query = new Query(Name, Page ?? 1, PageSize ?? 20);
                var result = Ok(query);
                return result;
            }
        }
    }
}
