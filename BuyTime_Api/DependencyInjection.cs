using BuyTime_Api.Common.Errors;
using BuyTime_Application.Dto;
using BuyTime_Domain.Constants;
using BuyTime_Domain.Entities;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Text;

namespace BuyTime_Api;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        services.AddSingleton<ProblemDetailsFactory, BuyTimeProblemDetailsFactory>();
        services.AddSwagger();
        services.AddMappings();
        services.AddAuth(configuration);

        return services;
    }

    private static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration config)
    {
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.UseSecurityTokenValidators = true;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(config["Jwt:Secret"] ?? "SuperSecretKeyForBuyTimeApp_MakeItLongEnough12345!"))
                };
            });

        return services;
    }

    private static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(option =>
        {
            option.SwaggerDoc("v1", new OpenApiInfo { Title = "Dashboard API", Version = "v1" });

            option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter a valid token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "Bearer"
            });
            option.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[]{}
                }
            });
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
                    .Where(ts => ts.Bookings != null && ts.Bookings.Any(b => b.Status == Status.Completed))
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
            //.Map(dest => dest.Feedbacks, src => src.ReceivedFeedbacks);

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
                    : (src.Status == Status.Expired ? "system" :
                       src.Status == Status.Rejected ? "expert" : null));

        TypeAdapterConfig<Timeslot, TimeslotDto>.NewConfig()
            .Map(dest => dest.Booking, src =>
                src.Bookings != null
                    ? src.Bookings.FirstOrDefault(b => b.Status == Status.Pending || b.Status == Status.Confirmed)
                    : null);

        TypeAdapterConfig<TransactionRecord, TransactionRecordDto>.NewConfig()
            .Map(dest => dest.BookingDetails, src => src.Booking);

        TypeAdapterConfig<Booking, TransactionBookingSummaryDto>.NewConfig()
            .Map(dest => dest.Status, src => src.Status)
            .Map(dest => dest.StartTime, src => src.TimeSlot != null ? src.TimeSlot.StartTime : (DateTime?)null)
            .Map(dest => dest.EndTime, src => src.TimeSlot != null ? src.TimeSlot.EndTime : (DateTime?)null)
            .Map(dest => dest.CancellationReason, src => src.Cancellation != null ? src.Cancellation.Reason : null)
            .Map(dest => dest.CancelledByRole, src =>
                src.Cancellation != null
                    ? (src.Cancellation.CancelledByUserId == src.StudentId ? "student" : "expert")
                    : (src.Status == Status.Expired ? "system" :
                       src.Status == Status.Rejected ? "expert" : null));

        config.Scan(Assembly.GetExecutingAssembly());

        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();

        return services;
    }
}