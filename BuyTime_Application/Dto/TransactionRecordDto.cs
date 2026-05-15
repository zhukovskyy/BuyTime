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
    public string? ExplorerUrl { get; set; }

    public Guid? BookingId { get; set; }

    public TransactionBookingSummaryDto? BookingDetails { get; set; }
}

public class TransactionBookingSummaryDto
{
    public string Status { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? CancellationReason { get; set; }
    public string? CancelledByRole { get; set; }
}