namespace DreamGuard.BE.DAL.Constants
{
    public enum OrderStatus
    {
        Pending = 0,
        Confirmed = 1,
        Processing = 2,
        Shipping = 3,
        Delivered = 4,
        Completed = 5,
        Cancelled = 6,
        Returned = 7,
        Returning = 8,
        RefundedAndRestocked = 9,
        RefundedAndDamaged = 10,
        ExchangeRequested = 11,
        Shipping_Replacement = 12
    }
}
