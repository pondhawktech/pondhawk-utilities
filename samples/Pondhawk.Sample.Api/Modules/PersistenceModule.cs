using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pondhawk.Configuration;
using Pondhawk.Persistence;
using Pondhawk.Sample.Api.Data;

namespace Pondhawk.Sample.Api.Modules;

/// <summary>
/// Persistence registration. Config-bound: <see cref="ConnectionString"/> is populated from the
/// configuration root and used to configure the EF Core context.
/// </summary>
public sealed class PersistenceModule : IServiceModule
{
    public string ConnectionString { get; set; } = "Data Source=widgets.db";

    public void Build(IServiceCollection services, IConfiguration configuration)
    {
        services.AddPondhawkDbContext<AppDbContext>(options => options.UseSqlite(ConnectionString));
    }
}
