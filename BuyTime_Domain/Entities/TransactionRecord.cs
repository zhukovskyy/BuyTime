using BuyTime_Domain.Enums;

namespace BuyTime_Domain.Entities;

public class TransactionRecord
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; }

    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }

    public string CounterpartyName { get; set; }
    public DateTime ExecutedAt { get; set; }

    public string? ContractAddress { get; set; }

    public Guid? BookingId { get; set; }
    public Booking? Booking { get; set; }
}