namespace BuyTime_Infrastructure.Common.Settings;

public class TonSettings
{
    public const string SectionName = "Ton";
    public string ApiKey { get; set; } = string.Empty;
    public bool IsTestnet { get; set; } = true;

    public string ContractCodeHex { get; set; } = string.Empty;
    public string ArbiterAddress { get; set; } = string.Empty;
    public string ArbiterMnemonic { get; set; } = string.Empty;
    public string PlatformAddress { get; set; } = string.Empty;
}