using System;
using System.Collections.Generic;

namespace BuyTime_Application.Dto;

public class ExpertProfileDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string? ExpertNickname { get; set; }
    public string? Description { get; set; }
    public string? Email { get; set; }
    public decimal? Rating { get; set; }
    //public string? Tags { get; set; } // remove later
    public string? AvatarUrl { get; set; } 

    
    public double TotalHoursConducted { get; set; }
    public int HappyStudentsCount { get; set; } // Rating >= 4
    public int ReviewCount { get; set; }

    
    public List<LanguageSkillDto> LanguageSkills { get; set; } = new();
    public List<SocialLinkDto> SocialLinks { get; set; } = new();
    public List<SpecializationDto> Specializations { get; set; } = new();
    public List<FeedbackDto> Feedbacks { get; set; } = new();
    public List<TimeslotDto> TimeSlots { get; set; } = new();
}