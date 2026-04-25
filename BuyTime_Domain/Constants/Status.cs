namespace BuyTime_Domain.Constants;

public class Status
{
    public const string PaymentPending = "paymentPending";
    public const string Pending = "pending";
    public const string Confirmed = "confirmed";
    public const string Cancelled = "cancelled";
    public const string CancelPending = "cancelPending";
    public const string Expired = "expired";
    public const string Rejected = "rejected";
    public const string RefundPending = "refundPending";
    public const string Refunded = "refunded";
    public const string CompletionPending = "completionPending";
    public const string Completed = "completed";

    public static readonly string[] All = new[] { 
        PaymentPending, Pending, Confirmed, 
        CancelPending, Cancelled, Expired, Rejected,
        CompletionPending, Completed, RefundPending, Refunded 
    };
}
