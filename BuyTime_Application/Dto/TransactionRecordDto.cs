namespace BuyTime_Application.Dto;

public class TransactionRecordDto
{
    public Guid Id { get; set; }

    public string Type { get; set; }

    public decimal Amount { get; set; }
    public string Currency { get; set; }

    public string CounterpartyName { get; set; }
    public DateTime ExecutedAt { get; set; }

    public string? ContractAddress { get; set; }
    public Guid? BookingId { get; set; }
}