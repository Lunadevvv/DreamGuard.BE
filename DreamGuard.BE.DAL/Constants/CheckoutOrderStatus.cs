namespace DreamGuard.BE.DAL.Constants
{
    public enum CheckoutOrderStatus
    {
        Pending = 0,
        Confirmed = 1,
        PartialRefunding = 2,
        PartialRefunded = 3,
        CancelledAndRefunded = 4,
        Cancelled = 5,
        Completed = 6
    }
}
