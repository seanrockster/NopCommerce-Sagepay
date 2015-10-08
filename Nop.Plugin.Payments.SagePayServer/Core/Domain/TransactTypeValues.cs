namespace Nop.Plugin.Payments.SagePayServer.Core.Domain
{
    public enum TransactTypeValues : int
    {
        Payment = 10,
        Deferred = 20,
        Authorize = 30,
    }
}