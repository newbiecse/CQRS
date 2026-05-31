namespace CqrsDemo.Domain.Orders;

public enum OrderStatus
{
    PendingPayment = 0,
    Paid = 1,
    Cancelled = 2
}
