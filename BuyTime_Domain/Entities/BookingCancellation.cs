namespace BuyTime_Domain.Entities;

public class BookingCancellation
{
    public Guid BookingId { get; set; }
    public Booking Booking { get; set; }
    public string Reason { get; set; }
    public DateTime CancelledAt { get; set; }
    public Guid CancelledByUserId { get; set; }
    public decimal RefundAmountToStudent { get; set; }
    public decimal CompensationAmountToExpert { get; set; }
}