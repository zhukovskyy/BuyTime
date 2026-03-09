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
            .Map(dest => dest.TimeSlots, src => src.TimeSlots.Where(ts => ts.IsAvailable))
            .Map(dest => dest.Specializations, src => src.Specializations)
            .Map(dest => dest.LanguageSkills, src => src.ExpertLanguages);

        TypeAdapterConfig<ExpertSocialLink, SocialLinkDto>.NewConfig()
            .Map(dest => dest.Platform, src => src.Platform.Name)
            .Map(dest => dest.LogoUrl, src => src.Platform.LogoUrl);

        TypeAdapterConfig<ExpertLanguage, LanguageSkillDto>.NewConfig()
            .Map(dest => dest.LanguageCode, src => src.Language.Code)
            .Map(dest => dest.Level, src => src.Level);

        TypeAdapterConfig<User, UserDto>.NewConfig()
            .Map(dest => dest.LanguageSkills, src => src.ExpertLanguages)
            .Map(dest => dest.Specializations, src => src.Specializations)
            .Map(dest => dest.SocialLinks, src => src.SocialLinks);

        TypeAdapterConfig<Booking, TimeslotBookingSummaryDto>.NewConfig()
            .Map(dest => dest.StudentFirstName, src => src.Student.FirstName)
            .Map(dest => dest.StudentLastName, src => src.Student.LastName);

        TypeAdapterConfig<Booking, StudentBookingSummaryDto>.NewConfig()
            .Map(dest => dest.ExpertFirstName, src => src.TimeSlot.Expert.FirstName)
            .Map(dest => dest.ExpertLastName, src => src.TimeSlot.Expert.LastName)

            .Map(dest => dest.TimeSlotStartTime, src => src.TimeSlot.StartTime)
            .Map(dest => dest.TimeSlotEndTime, src => src.TimeSlot.EndTime)
            .Map(dest => dest.TimeSlotPrice, src => src.TimeSlot.Price)
            .Map(dest => dest.TimeSlotCurrency, src => src.TimeSlot.Currency)

            .Map(dest => dest.CancellationReason, src => src.Cancellation != null ? src.Cancellation.Reason : null)
            .Map(dest => dest.CancelledByRole, src =>
                src.Cancellation != null
                    ? (src.Cancellation.CancelledByUserId == src.StudentId ? "student" : "expert")
                    : null);

        config.Scan(Assembly.GetExecutingAssembly());

        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();

        return services;
    }
}