using BuyTime_Api.Common.Errors;
using BuyTime_Application.Dto;
using BuyTime_Domain.Constants;
using BuyTime_Domain.Entities;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace BuyTime_Api;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddSingleton<ProblemDetailsFactory, BuyTimeProblemDetailsFactory>();
        services.AddSwagger();
        services.AddMappings();

        return services;
    }

    private static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(option =>
        {
            option.SwaggerDoc("v1", new OpenApiInfo { Title = "Dashboard API", Version = "v1" });
        });

        return services; 
    }
   
    public static IServiceCollection AddMappings(this IServiceCollection services)
    {
        var config = TypeAdapterConfig.GlobalSettings;

        TypeAdapterConfig<User, ExpertProfileDto>.NewConfig()
            .Map(dest => dest.HappyStudentsCount, src => src.ReceivedFeedbacks != null
                ? src.ReceivedFeedbacks.Count(f => f.Rating >= 4)
                : 0)
            .Map(dest => dest.ReviewCount, src => src.ReceivedFeedbacks != null ? src.ReceivedFeedbacks.Count : 0)

            .Map(dest => dest.TotalHoursConducted, src => src.TimeSlots != null
                ? src.TimeSlots
                    .Where(ts => ts.Booking != null && ts.Booking.Status == Status.Completed)
                    .Sum(ts => (ts.EndTime - ts.StartTime).TotalHours)
                : 0)

            .Map(dest => dest.Feedbacks, src => src.ReceivedFeedbacks)
            .Map(dest => dest.TimeSlots, src => src.TimeSlots.Where(ts => ts.IsAvailable));

        config.Scan(Assembly.GetExecutingAssembly());

        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();

        return services;
    }
}