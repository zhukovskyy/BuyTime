namespace BuyTime_Application.Expert.Query.Search;

public class SearchExpertRequest
{
    public string? SearchQuery { get; set; } // FirstName, LastName, ExpertNickname
    public string? Language { get; set; }
    public string? Specialization { get; set; } 

    
    public decimal? MinRating { get; set; }

    
    public decimal? MaxAveragePriceForFilter { get; set; }
    public string? Currency { get; set; }

    public bool OnlyFavorites { get; set; } = false;
    public Guid? CurrentUserId { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}