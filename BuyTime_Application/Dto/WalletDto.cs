namespace BuyTime_Application.Dto;

public class WalletDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Network { get; set; }
    public string Address { get; set; }
}