using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.SagePayServer.Core.Domain
{
    public class SagePayServerPaymentSettings : ISettings
    {
        /// <summary>
        /// Possible values are: SIMULATOR, TEST, LIVE
        /// </summary>
        public ConnectToValues ConnectTo { get; set; }

        /// <summary>
        /// Possible values are: PAYMENT, DEFERRED, AUTHENTICATE
        /// </summary>
        public TransactTypeValues TransactType { get; set; }

        /// <summary>
        /// Possible values are: LOW, NORMAL
        /// </summary>
        public ProfileValues Profile { get; set; }

        /// <summary>
        /// Identifies the SagePay vendor account who receives the payments
        /// </summary>
        public string VendorName { get; set; }

        /// <summary>
        /// Identifies the SagePay integrator (affiliate partner)
        /// </summary>
        public string PartnerId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to "additional fee" is specified as percentage. true - percentage, false - fixed value.
        /// </summary>
        public bool AdditionalFeePercentage { get; set; }

        /// <summary>
        /// Additional fee
        /// </summary>
        public decimal AdditionalFee { get; set; }

    }

    public enum ConnectToValues : int
    {
        Simulator = 10,
        Test = 20,
        Live = 30,
    }

    public enum TransactTypeValues : int
    {
        Payment = 10,
        Deferred = 20,
        Authorize = 30,
    }

    public enum ProfileValues : int
    {
        Low = 10,
        Normal = 20,
    }

}
