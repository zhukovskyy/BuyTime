namespace BuyTime_Domain.Constants;

public class Status
{
    public const string Pending = "pending";
    public const string Confirmed = "confirmed";
    public const string Cancelled = "cancelled";
    public static readonly string[] All = [Pending, Confirmed, Cancelled];
}