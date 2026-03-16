namespace BuyTime_Domain.Constants;

public class Status
{
    public const string Pending = "pending";
    public const string Confirmed = "confirmed";
    public const string Cancelled = "cancelled";
    public const string CancelPending = "cancelPending";
    public const string Rejected = "rejected";
    public const string Completed = "completed";
    public const string Refunded = "refunded";
    public static readonly string[] All = new[] { Pending, Confirmed, CancelPending, Cancelled, Rejected, Completed, Refunded };
}
