namespace CqrsDemo.Contracts.Saga;

public static class CheckoutSagaStates
{
    public const string Started = "Started";
    public const string CartCheckedOut = "CartCheckedOut";
    public const string OrderCreated = "OrderCreated";
    public const string PaymentPending = "PaymentPending";
    public const string Completed = "Completed";
    public const string PaymentFailed = "PaymentFailed";
    public const string Compensating = "Compensating";
    public const string Compensated = "Compensated";
    public const string Failed = "Failed";
}
