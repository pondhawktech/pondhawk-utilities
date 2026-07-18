# Pondhawk.Persistence

The EF Core persistence companion for Pondhawk Tools. It provides the **`IDbContext`** seam that the
application layer (vertical-slice handlers) depends on instead of a concrete `DbContext`, plus the DI
registration that wires a concrete context to that seam.

Standalone by design — depends only on `Microsoft.EntityFrameworkCore`. Like `Pondhawk.Api`, it follows
the **package-and-vendoring** model: reference it as a NuGet package, or vendor its source into a
consumer app and edit it in place. Namespace root: `Pondhawk.Persistence`.

## Why a seam

Slice handlers inject `IDbContext`, not your `AppDbContext`. That keeps handlers testable against a fake
and unbound from a specific context type, while your real context stays a normal EF Core `DbContext`.

```csharp
public class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options), IDbContext          // ← DbContext already provides Set<T>() + SaveChangesAsync
{
    public DbSet<Widget> Widgets => Set<Widget>();
}
```

```csharp
// Program.cs
builder.Services.AddPondhawkDbContext<AppDbContext>(o => o.UseSqlite(connectionString));
```

```csharp
// A slice handler depends only on the seam:
public class GetWidgetByIdHandler(IDbContext db) : IQueryHandler<GetWidgetByIdQuery, GetWidgetByIdResponse?>
{
    public Task<GetWidgetByIdResponse?> HandleAsync(GetWidgetByIdQuery q, CancellationToken ct) =>
        db.Set<Widget>()
          .Where(w => w.Uid == q.Uid)
          .Select(w => new GetWidgetByIdResponse(w.Uid, w.Name))
          .FirstOrDefaultAsync(ct);
}
```
