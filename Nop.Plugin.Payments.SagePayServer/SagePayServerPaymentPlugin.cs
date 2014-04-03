using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Routing;
using Nop.Core;
using Nop.Core.Domain;
using Nop.Core.Domain.Directory;
using Nop.Core.Plugins;
using Nop.Plugin.Payments.SagePayServer.Controllers;
using Nop.Plugin.Payments.SagePayServer.Core.Domain;
using Nop.Plugin.Payments.SagePayServer.Data;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Payments;
using System.Net;
using System.Globalization;
using System.Text;
using Nop.Core.Domain.Payments;
using Nop.Services.Orders;
using System.Web.Mvc;
using Nop.Web.Framework.Controllers;
using Nop.Plugin.Payments.SagePayServer.Services;
using Nop.Core.Domain.Orders;

namespace Nop.Plugin.Payments.SagePayServer
{
    public class SagePayServerPaymentPlugin : BasePlugin, IPaymentMethod
    {

        #region Fields

        private readonly ISagePayServerWorkflowService _sagePayServerWorkflowService;
        private readonly ISettingService _settingService;
        private readonly ISagePayServerTransactionService _sagePayServerTransactionService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly SagePayServerTransactionObjectContext _context;
        private readonly SagePayServerPaymentSettings _sagePayServerPaymentSettings;

        #endregion

        #region Ctor

        public SagePayServerPaymentPlugin(ISagePayServerWorkflowService sagePayServerWorkflowService, ISettingService settingService, 
            ISagePayServerTransactionService sagePayServerTransactionService, IOrderTotalCalculationService orderTotalCalculationService,
            SagePayServerTransactionObjectContext context, SagePayServerPaymentSettings sagePayServerPaymentSettings)
        {
            this._context = context;
            this._sagePayServerPaymentSettings = sagePayServerPaymentSettings;
            this._sagePayServerWorkflowService = sagePayServerWorkflowService;
            this._settingService = settingService;
            this._sagePayServerTransactionService = sagePayServerTransactionService;
            this._orderTotalCalculationService = orderTotalCalculationService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Process a payment right after order is created
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();

            var transx = _sagePayServerTransactionService.GetSagePayServerTransactionByVendorTxCode(processPaymentRequest.OrderGuid.ToString());

            if (transx == null)
            {
                result.AddError(String.Format("SagePay Server transaction code {0} does not exist.", processPaymentRequest.OrderGuid));
                return result;
            }

            if ((transx.Status == "OK") || (transx.Status == "AUTHENTICATED") || (transx.Status == "REGISTERED"))
            {
                switch (_sagePayServerPaymentSettings.TransactType)
                {
                    case TransactTypeValues.Payment:
                        var releaseResult = _sagePayServerWorkflowService.ReleaseTransaction(processPaymentRequest.OrderGuid.ToString(), processPaymentRequest.OrderTotal);

                        if (releaseResult.Success)
                        {
                            result.NewPaymentStatus = PaymentStatus.Paid;
                            result.CaptureTransactionResult = releaseResult.Message;
                        }
                        else
                        {
                            result.AddError(releaseResult.Message);
                        }

                        break;
                    case TransactTypeValues.Deferred:
                        result.NewPaymentStatus = PaymentStatus.Authorized;
                        break;
                    default:
                        result.NewPaymentStatus = PaymentStatus.Pending;
                        break;
                }

                result.AuthorizationTransactionId = transx.Id.ToString(CultureInfo.InvariantCulture);
                result.AuthorizationTransactionCode = transx.VpsTxId;
                result.AuthorizationTransactionResult = transx.ToString();
            }
            else
            {
                result.AddError(transx.StatusDetail);
            }

            
            return result;
            
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>Additional handling fee</returns>
        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            var result = this.CalculateAdditionalFee(_orderTotalCalculationService, cart,
                _sagePayServerPaymentSettings.AdditionalFee, _sagePayServerPaymentSettings.AdditionalFeePercentage);
            return result;
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            var result = new CapturePaymentResult();

            var releaseResult = _sagePayServerWorkflowService.ReleaseTransaction(capturePaymentRequest.Order.OrderGuid.ToString(), capturePaymentRequest.Order.OrderTotal);

            if (!releaseResult.Success)
            {
                result.AddError(releaseResult.Message);
            }
            else
            {
                result.NewPaymentStatus = PaymentStatus.Paid;
                result.CaptureTransactionResult = releaseResult.Message;
            }

            return result;
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            var result = new RefundPaymentResult();

            var refundResult = _sagePayServerWorkflowService.RefundTransaction(refundPaymentRequest.Order.OrderGuid.ToString(), refundPaymentRequest.Order.OrderTotal, refundPaymentRequest.Order.CustomerCurrencyCode);

            if (!refundResult.Success)
            {
                result.AddError(refundResult.Message);
            }
            else
            {
                result.NewPaymentStatus = PaymentStatus.Refunded;
            }

            return result;
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            var result = new VoidPaymentResult();

            var voidResult = _sagePayServerWorkflowService.VoidTransaction(voidPaymentRequest.Order.OrderGuid.ToString());

            if (!voidResult.Success)
            {
                result.AddError(voidResult.Message);
            }
            else
            {
                result.NewPaymentStatus = PaymentStatus.Voided;
            }

            return result;
        }

        /// <summary>
        /// Process recurring payment. Not supported in Sage Pay
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.AddError("Recurring payment not supported");
            return result;
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            var result = new CancelRecurringPaymentResult();
            result.AddError("Recurring payment not supported");
            return result;
        }


        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        public bool CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            //it's not a redirection payment method. So we always return false
            return false;
        }

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "PaymentSagePayServer";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Payments.SagePayServer.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// Gets a route for payment info
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PaymentInfo";
            controllerName = "PaymentSagePayServer";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Payments.SagePayServer.Controllers" }, { "area", null } };
        }

        public Type GetControllerType()
        {
            return typeof(PaymentSagePayServerController);
        }

        public override void Install()
        {
            var settings = new SagePayServerPaymentSettings()
            {
                TransactType = TransactTypeValues.Payment,
                ConnectTo = ConnectToValues.Simulator,
                Profile = ProfileValues.Normal                
            };
            _settingService.SaveSetting(settings);

            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.SagePayServer.Fields.ConnectTo", "Connect To");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.SagePayServer.Fields.ConnectTo.Hint", "Connect to test, simulator or live");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.SagePayServer.Fields.TransactType", "TransactType");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.SagePayServer.Fields.TransactType.Hint", "Transaction Type.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.SagePayServer.Fields.Profile", "Profile");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.SagePayServer.Fields.Profile.Hint", "Iframe or separate window");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.SagePayServer.Fields.VendorName", "Vendor Name");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.SagePayServer.Fields.VendorName.Hint", "Vendor Name.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.SagePayServer.Fields.PartnerId", "PartnerId");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.SagePayServer.Fields.PartnerId.Hint", "Affiliate Partner Id");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.SagePayServer.Fields.AdditionalFee", "Additional fee");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.SagePayServer.Fields.AdditionalFee.Hint", "Enter additional fee to charge your customers.");

            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.SagePayServer.PaymentInfoError", "There were errors while registering a new transaction. Please try again. If you continue to have problems please contact us and we will be able to assist you.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.SagePayServer.ResponsePageError", "There were errors while processing your payment. Please try again. If you continue to have problems please contact us and we will be able to assist you.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.SagePayServer.PaymentMade", "Your payment has been made.");
          
            _context.InstallSchema();

            base.Install();
        }

        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<SagePayServerPaymentSettings>();

            //locales
            this.DeletePluginLocaleResource("Plugins.Payments.SagePayServer.Fields.ConnectTo");
            this.DeletePluginLocaleResource("Plugins.Payments.SagePayServer.Fields.ConnectTo.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.SagePayServer.Fields.TransactType");
            this.DeletePluginLocaleResource("Plugins.Payments.SagePayServer.Fields.TransactType.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.SagePayServer.Fields.Profile");
            this.DeletePluginLocaleResource("Plugins.Payments.SagePayServer.Fields.Profile.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.SagePayServer.Fields.VendorName");
            this.DeletePluginLocaleResource("Plugins.Payments.SagePayServer.Fields.VendorName.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.SagePayServer.Fields.PartnerId");
            this.DeletePluginLocaleResource("Plugins.Payments.SagePayServer.Fields.PartnerId.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.SagePayServer.Fields.AdditionalFee");
            this.DeletePluginLocaleResource("Plugins.Payments.SagePayServer.Fields.AdditionalFee.Hint");

            this.DeletePluginLocaleResource("Plugins.Payments.SagePayServer.PaymentInfoError");
            this.DeletePluginLocaleResource("Plugins.Payments.SagePayServer.ResponsePageError");
            this.DeletePluginLocaleResource("Plugins.Payments.SagePayServer.PaymentMade");

            base.Uninstall();
        }

        #endregion

        #region Properies

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType
        {
            get
            {
                return RecurringPaymentType.NotSupported;
            }
        }

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType
        {
            get
            {
                return PaymentMethodType.Standard;
            }
        }

        public bool SkipPaymentInfo
        {
            get
            {
                return false;
            }
        }

        #endregion


    }
}