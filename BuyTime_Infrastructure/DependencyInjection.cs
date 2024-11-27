using BuyTime_Application.Common.Interfaces.IRepository;
using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Infrastructure.Common.Persistence;
using BuyTime_Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuyTime_Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
       this IServiceCollection services,
       ConfigurationManager configuration)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services
            .AddPersistence(configuration)
            .AddRepositories();

        return services;
    }

    private static IServiceCollection AddPersistence(
       this IServiceCollection services,
       IConfiguration configuration)
    {
        string connStr = configuration.GetConnectionString("DefaultConnection")!;

        services.AddDbContext<BuyTimeDbContext>(opt =>
        {
            opt.UseSqlServer(connStr);

            opt.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });


        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<ITeacherRepository, TeacherRepository>();
        return services;
    }
}