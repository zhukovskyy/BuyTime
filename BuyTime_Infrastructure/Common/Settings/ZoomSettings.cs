namespace BuyTime_Infrastructure.Common.Settings;

public class ZoomSettings
{
    public const string SectionName = "Zoom";
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string AccountId { get; set; } = string.Empty;
}