using Microsoft.Extensions.DependencyInjection;
using OutEHR.Application.Interfaces;
using OutEHR.Infrastructure.Data;
using OutEHR.Infrastructure.Repositories;

namespace OutEHR.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        Dapper.SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());

        services.AddSingleton(new DbConnectionFactory(connectionString));

        services.AddScoped<ISpecialtyRepository, SpecialtyRepository>();
        services.AddScoped<IClinicRepository, ClinicRepository>();
        services.AddScoped<IProviderRepository, ProviderRepository>();
        services.AddScoped<IClientRepository, ClientRepository>();

        return services;
    }
}
