namespace BuyTime_Domain.Entities;

public class Wallet
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string WalletType { get; set; } // Phantom, solflare etc.
    public string WalletAddress { get; set; }
    public User User { get; set; }
}