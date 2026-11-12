using BibliotekaIznajmljivanje.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace BibliotekaIznajmljivanje.Api.Extensions;

public static class DatabaseExtensions
{
    public static IServiceCollection AddBibliotekaDataBase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<BibliotekaDbContext>(options=>options.UseSqlite(configuration.GetConnectionString("BazaBiblioteka")));
        return services;
    }
}
