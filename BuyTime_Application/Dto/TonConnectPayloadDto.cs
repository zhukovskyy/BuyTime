namespace BuyTime_Application.Dto;

public class TonConnectPayloadDto
{
    public string ContractAddress { get; set; }
    public string StateInitBase64 { get; set; }
    public string PayloadBase64 { get; set; }
    public string AmountNanoTon { get; set; }
}