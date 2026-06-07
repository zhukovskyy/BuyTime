namespace BuyTime_Domain.Entities;

public class RefundRequest
{
    public Guid BookingId { get; set; }
    public Booking Booking { get; set; }

    public decimal Amount { get; set; }
    public DateTime RequestedAt { get; set; }
    public string PreviousStatus { get; set; }
}