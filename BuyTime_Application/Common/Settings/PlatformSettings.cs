namespace BuyTime_Application.Common.Settings;

public class PlatformSettings
{
    public const string SectionName = "PlatformSettings";
    public decimal MinTimeslotPriceTon { get; set; }
    public decimal CommissionPercent { get; set; }
}