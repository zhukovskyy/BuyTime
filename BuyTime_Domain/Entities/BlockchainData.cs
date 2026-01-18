namespace BuyTime_Domain.Entities;

public class BlockchainData
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string? Address { get; set; }

    public string? Mnemonic { get; set; }
}