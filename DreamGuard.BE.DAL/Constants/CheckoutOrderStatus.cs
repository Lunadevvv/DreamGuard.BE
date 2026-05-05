namespace DreamGuard.BE.DAL.Constants
{
    public enum CheckoutOrderStatus
    {
        Pending = 0,
        Confirmed = 1,
        PartialRefunding = 2,
        PartialRefunded = 3,
        CancelledAndRefunding = 4,
        CancelledAndRefunded = 5,
        Cancelled = 6,
        Completed = 7,
        ReturnedAndRefunded = 8,
        PartialCompleted = 9
    }
}
