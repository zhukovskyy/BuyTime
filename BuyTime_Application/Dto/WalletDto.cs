namespace BuyTime_Application.Dto;

public class WalletDto
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string WalletType {  get; set; }
    public string WalletAddress { get; set; }
}