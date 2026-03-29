using BuyTime_Application.Common.Interfaces.IRepository;
using BuyTime_Application.Common.Interfaces.IService;
using BuyTime_Application.Common.Interfaces.IUnitOfWork;
using BuyTime_Infrastructure.Common.Settings;
using BuyTime_Infrastructure.Common.Persistence;
using BuyTime_Infrastructure.Repositories;
using BuyTime_Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BuyTime_Application.Common.Settings;

namespace BuyTime_Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
       this IServiceCollection services,
       ConfigurationManager configuration)
    {
        services.Configure<ZoomSettings>(configuration.GetSection(ZoomSettings.SectionName));
        services.Configure<DiscordSettings>(configuration.GetSection(DiscordSettings.SectionName));
        services.Configure<TonSettings>(configuration.GetSection(TonSettings.SectionName));
        services.Configure<TelegramSettings>(configuration.GetSection(TelegramSettings.SectionName));
        services.Configure<PlatformSettings>(configuration.GetSection(PlatformSettings.SectionName));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services
            .AddPersistence(configuration)
            .AddRepositories()
            .AddServices();

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

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<ITelegramService, TelegramService>();
        services.AddTransient<TelegramService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddTransient<BookingService>();
        services.AddHttpClient<IZoomService, ZoomService>();
        services.AddScoped<IBlockchainService, BlockchainService>();
        services.AddScoped<IImageService, ImageService>();

        services.AddSingleton<DiscordBotService>();
        services.AddHostedService(provider => provider.GetRequiredService<DiscordBotService>());
        services.AddSingleton<IDiscordService>(provider => provider.GetRequiredService<DiscordBotService>());

        services.AddScoped<ITonContractService, TonContractService>();
        services.AddHostedService<TonContractMonitorService>();

        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITimeSlotRepository, TimeslotRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<IFeedbackRepository, FeedbackRepository>();
        services.AddScoped<IFavoriteExpertRepository, FavoriteExpertRepository>();
        services.AddScoped<IUserSettingsRepository, UserSettingsRepository>();
        return services;
    }
}