namespace BuyTime_Application.Expert.Query.Search;

public class SearchExpertRequest // це для фільтра
{
    public string? SearchQuery { get; set; } // FirstName, LastName, ExpertNickname
    public string? Language { get; set; }
    public string? Specialization { get; set; } // Tags

    
    public decimal? MinRating { get; set; }

    
    public decimal? MaxAveragePriceForFilter { get; set; }
    public string? Currency { get; set; } 
}