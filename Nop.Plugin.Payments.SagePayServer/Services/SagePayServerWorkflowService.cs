using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.SagePayServer.Core;
using Nop.Plugin.Payments.SagePayServer.Core.Domain;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Orders;
using Nop.Services.Payments;

namespace Nop.Plugin.Payments.SagePayServer.Services
{
    public partial class SagePayServerWorkflowService : ISagePayServerWorkflowService
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly ICurrencyService _currencyService;
        private readonly IWebHelper _webHelper;
        private readonly ISagePayServerTransactionService _sagePayServerTransactionService;

        private readonly HttpRequestBase _request;
        private readonly CurrencySettings _currencySettings;
        private readonly SagePayServerPaymentSettings _sagePayServerPaymentSettings;

        #endregion

        #region Ctor

        public SagePayServerWorkflowService(IWebHelper webHelper,IWorkContext workContext, 
            ISagePayServerTransactionService sagePayServerTransactionService,
            IOrderTotalCalculationService orderTotalCalculationService, ICurrencyService currencyService,
            HttpRequestBase request,
            CurrencySettings currencySettings, SagePayServerPaymentSettings sagePayServerPaymentSettings)
        {
            this._webHelper = webHelper;
            this._sagePayServerTransactionService = sagePayServerTransactionService;
            this._orderTotalCalculationService = orderTotalCalculationService;
            this._currencyService = currencyService;
            this._sagePayServerPaymentSettings = sagePayServerPaymentSettings;
            this._currencySettings = currencySettings;
            this._workContext = workContext;
            this._request = request;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Register a new transaction with Sagepay before the iframe shows. In fact, this method should give us the URL for the iframe
        /// </summary>
        /// <returns></returns>
        public RegisterTransactionResult RegisterTransaction()
        {
            var result = new RegisterTransactionResult();

            if (_request.IsLocal)
            {
                result.Message =
                    "This plugin does not work in local hosts. You need to publish NopCommerce on a publicly accessible internet address.";
                return result;
            }

            var webClient = new WebClient();

            var orderGuid = Guid.NewGuid();

            var data = new QueryStringNameValueCollection
                           {
                               {"VPSProtocol", SagePayHelper.GetProtocol()},
                               {"TxType", TransactTypeValues.Deferred.ToString()},//we always use Deferred because we only take the payment after user has confirmed
                               {"Vendor", _sagePayServerPaymentSettings.VendorName.ToLower()},
                               {"VendorTxCode", orderGuid.ToString()}
                           };

            if (!String.IsNullOrWhiteSpace(_sagePayServerPaymentSettings.PartnerId))
                data.Add("ReferrerID", _sagePayServerPaymentSettings.PartnerId);

            var cart = _workContext.CurrentCustomer.ShoppingCartItems.ToList();
            cart = cart.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList();

            var orderTotal = _orderTotalCalculationService.GetShoppingCartTotal(cart).GetValueOrDefault();

            data.Add("Amount", orderTotal.ToString("F2", CultureInfo.InvariantCulture));

            data.Add("Currency", _workContext.WorkingCurrency != null
                         ? _workContext.WorkingCurrency.CurrencyCode
                         : _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode);

            data.Add("Description", "eCommerce Order from " + _sagePayServerPaymentSettings.VendorName);

            // The Notification URL is the page to which Server calls back when a transaction completes
            data.Add("NotificationURL", _webHelper.GetStoreLocation() + "Plugins/PaymentSagePayServer/NotificationPage");

            // Billing Details
            data.Add("BillingSurname", _workContext.CurrentCustomer.BillingAddress.LastName);
            data.Add("BillingFirstnames", _workContext.CurrentCustomer.BillingAddress.FirstName);
            data.Add("BillingAddress1", _workContext.CurrentCustomer.BillingAddress.Address1);

            if (!String.IsNullOrWhiteSpace(_workContext.CurrentCustomer.BillingAddress.Address2))
                data.Add("BillingAddress2", _workContext.CurrentCustomer.BillingAddress.Address2);

            data.Add("BillingCity", _workContext.CurrentCustomer.BillingAddress.City);
            data.Add("BillingPostCode", _workContext.CurrentCustomer.BillingAddress.ZipPostalCode);
            data.Add("BillingCountry", _workContext.CurrentCustomer.BillingAddress.Country.TwoLetterIsoCode); //TODO: Verify if it is ISO 3166-1 country code

            if (_workContext.CurrentCustomer.BillingAddress.StateProvince != null && _workContext.CurrentCustomer.BillingAddress.Country.TwoLetterIsoCode.ToLower() == "us")
            {
                var state = _workContext.CurrentCustomer.BillingAddress.StateProvince.Abbreviation;
                data.Add("BillingState", (state.Length > 2) ? state.Substring(0, 2) : state);
            }

            if (!String.IsNullOrWhiteSpace(_workContext.CurrentCustomer.BillingAddress.PhoneNumber))
                data.Add("BillingPhone", _workContext.CurrentCustomer.BillingAddress.PhoneNumber);



            // Delivery Details
            if (_workContext.CurrentCustomer.ShippingAddress != null)
            {
                data.Add("DeliverySurname", _workContext.CurrentCustomer.ShippingAddress.LastName);
                data.Add("DeliveryFirstnames", _workContext.CurrentCustomer.ShippingAddress.FirstName);
                data.Add("DeliveryAddress1", _workContext.CurrentCustomer.ShippingAddress.Address1);

                if (!String.IsNullOrWhiteSpace(_workContext.CurrentCustomer.ShippingAddress.Address2))
                    data.Add("DeliveryAddress2", _workContext.CurrentCustomer.ShippingAddress.Address2);

                data.Add("DeliveryCity", _workContext.CurrentCustomer.ShippingAddress.City);
                data.Add("DeliveryPostCode", _workContext.CurrentCustomer.ShippingAddress.ZipPostalCode);

                if (_workContext.CurrentCustomer.ShippingAddress.Country != null)
                {
                    data.Add("DeliveryCountry", _workContext.CurrentCustomer.ShippingAddress.Country.TwoLetterIsoCode);

                    if (_workContext.CurrentCustomer.ShippingAddress.StateProvince != null && _workContext.CurrentCustomer.ShippingAddress.Country.TwoLetterIsoCode.ToLower() == "us")
                    {
                        var state = _workContext.CurrentCustomer.ShippingAddress.StateProvince.Abbreviation;
                        data.Add("DeliveryState", (state.Length > 2) ? state.Substring(0, 2) : state);
                    }
                }

                if (!String.IsNullOrWhiteSpace(_workContext.CurrentCustomer.ShippingAddress.PhoneNumber))
                    data.Add("DeliveryPhone", _workContext.CurrentCustomer.ShippingAddress.PhoneNumber);

            }
            else
            {
                data.Add("DeliverySurname", "");
                data.Add("DeliveryFirstnames", "");
                data.Add("DeliveryAddress1", "");
                data.Add("DeliveryAddress2", "");
                data.Add("DeliveryCity", "");
                data.Add("DeliveryPostCode", "");
                data.Add("DeliveryCountry", "");
                data.Add("DeliveryState", "");
                data.Add("DeliveryPhone", "");
            }

            data.Add("CustomerEMail", _workContext.CurrentCustomer.Email);

            //var strBasket = String.Empty;
            //strBasket = cart.Count + ":";

            //for (int i = 0; i < cart.Count; i++)
            //{
            //    ShoppingCartItem item = cart[i];
            //    strBasket += item.ProductVariant.FullProductName) + ":" +
            //                    item.Quantity + ":" + item.ProductVariant.Price + ":" +
            //                    item.ProductVariant.TaxCategoryId;
            //};

            //data.Add("Basket", strBasket);

            data.Add("AllowGiftAid", "0");

            // Allow fine control over AVS/CV2 checks and rules by changing this value. 0 is Default
            //data.Add("ApplyAVSCV2", "0");

            // Allow fine control over 3D-Secure checks and rules by changing this value. 0 is Default
            //data.Add("Apply3DSecure", "0");

            if (_sagePayServerPaymentSettings.Profile == ProfileValues.Low)
            {
                data.Add("Profile", "LOW"); //simpler payment page version.
            }

            var sageSystemUrl = SagePayHelper.GetSageSystemUrl(_sagePayServerPaymentSettings.ConnectTo, "purchase");

            string strResponse;

            try
            {

                var responseData = webClient.UploadValues(sageSystemUrl, data);

                strResponse = Encoding.ASCII.GetString(responseData);
            }
            catch (WebException ex)
            {

                result.Message= String.Format(@"Your server was unable to register this transaction with Sage Pay.
                    Check that you do not have a firewall restricting the POST and 
                    that your server can correctly resolve the address {0}. <br/>
                    The Status Number is: {1}<br/>
                    The Description given is: {2}", sageSystemUrl, ex.Status, ex.Message);

                return result;

            }

            if (!string.IsNullOrWhiteSpace(strResponse))
            {

                var strStatus = SagePayHelper.FindField("Status", strResponse);
                var strStatusDetail = SagePayHelper.FindField("StatusDetail", strResponse);

                switch (strStatus)
                {
                    case "OK":

                        var strVpsTxId = SagePayHelper.FindField("VPSTxId", strResponse);
                        var strSecurityKey = SagePayHelper.FindField("SecurityKey", strResponse);
                        var strNextUrl = SagePayHelper.FindField("NextURL", strResponse);

                        var transx = new SagePayServerTransaction
                        {
                            CreatedOnUtc = DateTime.UtcNow,
                            VpsTxId = strVpsTxId,
                            SecurityKey = strSecurityKey,
                            NotificationResponse = strResponse.Replace(Environment.NewLine, ";"),
                            VendorTxCode = orderGuid.ToString(),
                        };

                        //Store this record in DB
                        _sagePayServerTransactionService.InsertSagePayServerTransaction(transx);

                        result.Success = true;
                        result.PaymentUrl = strNextUrl;

                        return result;

                    case "MALFORMED":

                        result.Message = string.Format("Error {0}, {1} - {2}", strStatus, strStatusDetail, data.Encode());

                        break;

                    case "INVALID":

                        result.Message = string.Format("Error {0}, {1} - {2}", strStatus, strStatusDetail, data.Encode());

                        break;

                    default:

                        result.Message = string.Format("Error {0}, {1}", strStatus, strStatusDetail);

                        break;
                }

            }

            return result;
        }

        /// <summary>
        /// This method is called when payment is submitted inside the iframe. This method is called from Sagepay not from us, that's why the response is a string what Sagepay is expected. 
        /// This page is required to validate if the website that originated the transaction is the same one receiving the parameters (to avoid tampering with)
        /// </summary>
        /// <param name="strTxAuthNo"></param>
        /// <param name="strAvscv2"></param>
        /// <param name="strAddressResult"></param>
        /// <param name="strPostCodeResult"></param>
        /// <param name="strCv2Result"></param>
        /// <param name="strGiftAid"></param>
        /// <param name="str3DSecureStatus"></param>
        /// <param name="strCavv"></param>
        /// <param name="strAddressStatus"></param>
        /// <param name="strPayerStatus"></param>
        /// <param name="strCardType"></param>
        /// <param name="strLast4Digits"></param>
        /// <param name="strVpsTxId"></param>
        /// <param name="strVpsSignature"></param>
        /// <param name="strStatus"></param>
        /// <param name="strStatusDetail"></param>
        /// <param name="strVendorTxCode"></param>
        /// <returns></returns>
        public string ValidateTransaction(string strTxAuthNo,string strAvscv2,string strAddressResult,string strPostCodeResult,string strCv2Result,string strGiftAid,string str3DSecureStatus,string strCavv,
            string strAddressStatus,string strPayerStatus,string strCardType,string strLast4Digits,string strVpsTxId,string strVpsSignature,string strStatus,string strStatusDetail,string strVendorTxCode)
        {

            var strVendorName = _sagePayServerPaymentSettings.VendorName.ToLower();

            //Obtain from DB
            var transx = _sagePayServerTransactionService.GetSagePayServerTransactionByVendorTxCode(strVendorTxCode);

            if (transx == null)
            {
                strStatusDetail = "Vendor Transaction code " + strVendorTxCode + " does not exist.";

                return "Status=INVALID" + Environment.NewLine +
                               "RedirectURL=" + _webHelper.GetStoreLocation() + "Plugins/PaymentSagePayServer/ResponsePage?uid=" + strVendorTxCode + Environment.NewLine +
                               "StatusDetail=" + strStatusDetail;
            }

            if (string.IsNullOrWhiteSpace(transx.SecurityKey))
            {
                strStatusDetail = "Security Key for transaction " + strVendorTxCode + " is empty.";


                return "Status=INVALID" + Environment.NewLine +
                               "RedirectURL=" + _webHelper.GetStoreLocation() + "Plugins/PaymentSagePayServer/ResponsePage?uid=" + strVendorTxCode + Environment.NewLine +
                               "StatusDetail=" + strStatusDetail;
            }



            //Update DB with what we've got so far
            transx.VpsTxId = strVpsTxId;
            transx.VpsSignature = strVpsSignature;
            transx.Status = strStatus;
            transx.StatusDetail = strStatusDetail;
            transx.TxAuthNo = strTxAuthNo;
            transx.Avscv2 = strAvscv2;
            transx.AddressResult = strAddressResult;
            transx.PostCodeResult = strPostCodeResult;
            transx.Cv2Result = strCv2Result;
            transx.GiftAid = strGiftAid;
            transx.ThreeDSecureStatus = str3DSecureStatus;
            transx.Cavv = strCavv;
            transx.AddressStatus = strAddressStatus;
            transx.PayerStatus = strPayerStatus;
            transx.CardType = strCardType;
            transx.Last4Digits = strLast4Digits;

            //Update DB with what we've got so far
            _sagePayServerTransactionService.UpdateSagePayServerTransaction(transx);

            var strMessage = strVpsTxId + strVendorTxCode + strStatus + strTxAuthNo + strVendorName + strAvscv2 + transx.SecurityKey +
               strAddressResult + strPostCodeResult + strCv2Result + strGiftAid + str3DSecureStatus + strCavv +
               strAddressStatus + strPayerStatus + strCardType + strLast4Digits;

            //Because Sagepay also hashed all these variables, we also need to do the same to verify that they are the same
            var strMySignature = SagePayHelper.HashMd5(strMessage);

            if (strMySignature != strVpsSignature)
            {
                transx.StatusDetail = "Your server was unable to register this transaction with Sage Pay. Cannot match the MD5 Hash. Order might be tampered with: " + strMessage;

                _sagePayServerTransactionService.UpdateSagePayServerTransaction(transx);

                return "Status=INVALID" + Environment.NewLine +
                               "RedirectURL=" + _webHelper.GetStoreLocation() + "Plugins/PaymentSagePayServer/ResponsePage?uid=" + strVendorTxCode + Environment.NewLine +
                               "StatusDetail=Your server was unable to register this transaction with Sage Pay. Cannot match the MD5 Hash. Order might be tampered with";
            }

            //Always send a Status of OK if we've read everything correctly. Only INVALID for messages with a Status of ERROR
            var responseStatus = strStatus == "ERROR" ? "INVALID" : "OK";

            return "Status=" + responseStatus + Environment.NewLine +
                           "RedirectURL=" + _webHelper.GetStoreLocation() + "Plugins/PaymentSagePayServer/ResponsePage?uid=" + strVendorTxCode;
        }

        /// <summary>
        /// Release an order that was deferred
        /// </summary>
        /// <param name="orderGuid">Order guid</param>
        /// <param name="orderTotal">Order total</param>
        /// <returns></returns>
        public ReleaseTransactionResult ReleaseTransaction(String orderGuid, decimal orderTotal)
        {
            var result = new ReleaseTransactionResult();

            var transx = _sagePayServerTransactionService.GetSagePayServerTransactionByVendorTxCode(orderGuid);

            if (transx == null)
            {
                result.Message = String.Format("SagePay Server vendor transaction code {0} does not exist.", orderGuid);
                return result;
            }


            var webClient = new WebClient();

            var data = new QueryStringNameValueCollection
                           {
                               {"VPSProtocol", SagePayHelper.GetProtocol()},
                               {"TxType", "RELEASE"},
                               {"Vendor", _sagePayServerPaymentSettings.VendorName},
                               {"VendorTxCode", orderGuid.ToString()},
                               {"VPSTxId", transx.VpsTxId},
                               {"SecurityKey", transx.SecurityKey},
                               {"TxAuthNo", transx.TxAuthNo},
                               {
                                   "ReleaseAmount",
                                   orderTotal.ToString("F2", CultureInfo.InvariantCulture)
                               }
                           };

            var postUrl = SagePayHelper.GetSageSystemUrl(_sagePayServerPaymentSettings.ConnectTo, "release");

            string strResponse;

            try
            {

                var responseData = webClient.UploadValues(postUrl, data);

                strResponse = Encoding.ASCII.GetString(responseData);


            }
            catch (WebException ex)
            {
                result.Message = (String.Format(
                    @"Your server was unable to release this transaction with Sage Pay.
                    Check that you do not have a firewall restricting the POST and 
                    that your server can correctly resolve the address {0}. <br/>
                    The Status Number is: {1}<br/>
                    The Description given is: {2}", postUrl, ex.Status, ex.Message));
                return result;
            }

            if (string.IsNullOrWhiteSpace(strResponse))
            {
                result.Message = (String.Format(
                    @"Your server was unable to register this transaction with Sage Pay.
                    Check that you do not have a firewall restricting the POST and 
                    that your server can correctly resolve the address {0}.", postUrl));
                return result;
            }
            var strStatus = SagePayHelper.FindField("Status", strResponse);
            var strStatusDetail = SagePayHelper.FindField("StatusDetail", strResponse);

            switch (strStatus)
            {
                case "OK":

                    result.Success = true;
                    result.Message = strStatusDetail;
                    break;

                case "MALFORMED":
                    result.Message = (string.Format("Error ({0}: {1}) <br/> {2}", strStatus, strStatusDetail, data.Encode()));
                    break;

                case "INVALID":
                    result.Message = (string.Format("Error ({0}: {1}) <br/> {2}", strStatus, strStatusDetail, data.Encode()));
                    break;

                default:
                    result.Message = (string.Format("Error ({0}: {1})", strStatus, strStatusDetail));
                    break;

            }

            return result;
        }

        /// <summary>
        /// Refund an order that has been paid or released
        /// </summary>
        /// <param name="orderGuid">Order guid</param>
        /// <param name="orderTotal">Order total</param>
        /// <param name="currencyCode">Currency code</param>
        /// <returns></returns>
        public RefundTransactionResult RefundTransaction(String orderGuid, decimal orderTotal, string currencyCode)
        {
            var result = new RefundTransactionResult();

            var transx = _sagePayServerTransactionService.GetSagePayServerTransactionByVendorTxCode(orderGuid);

            if (transx == null)
            {
                result.Message = String.Format("SagePay Server vendor transaction code {0} does not exist.", orderGuid);
                return result;
            }


            var webClient = new WebClient();
            var returnGuid = Guid.NewGuid();

            var data = new QueryStringNameValueCollection
                           {
                               {"VPSProtocol", SagePayHelper.GetProtocol()},
                               {"TxType", "REFUND"},
                               {"Vendor", _sagePayServerPaymentSettings.VendorName},
                               {"VendorTxCode", returnGuid.ToString()},
                               {"VPSTxId", transx.VpsTxId},
                               {"SecurityKey", transx.SecurityKey},
                               {"TxAuthNo", transx.TxAuthNo},
                               {
                                   "Amount",
                                   orderTotal.ToString("F2", CultureInfo.InvariantCulture)
                               },
                               {"Currency", currencyCode},
                               {"Description", "---"},
                               {"RelatedVPSTxId", transx.VpsTxId},
                               {"RelatedVendorTxCode", orderGuid},
                               {"RelatedSecurityKey", transx.SecurityKey},
                               {"RelatedTxAuthNo", transx.TxAuthNo}
                           };

            var postUrl = SagePayHelper.GetSageSystemUrl(_sagePayServerPaymentSettings.ConnectTo, "refund");

            string strResponse;

            try
            {

                var responseData = webClient.UploadValues(postUrl, data);

                strResponse = Encoding.ASCII.GetString(responseData);


            }
            catch (WebException ex)
            {
                result.Message = (String.Format(
                    @"Your server was unable to release this transaction with Sage Pay.
                    Check that you do not have a firewall restricting the POST and 
                    that your server can correctly resolve the address {0}. <br/>
                    The Status Number is: {1}<br/>
                    The Description given is: {2}", postUrl, ex.Status, ex.Message));
                return result;
            }

            if (string.IsNullOrWhiteSpace(strResponse))
            {
                result.Message = (String.Format(
                    @"Your server was unable to register this transaction with Sage Pay.
                    Check that you do not have a firewall restricting the POST and 
                    that your server can correctly resolve the address {0}.", postUrl));
                return result;
            }
            var strStatus = SagePayHelper.FindField("Status", strResponse);
            var strStatusDetail = SagePayHelper.FindField("StatusDetail", strResponse);

            switch (strStatus)
            {
                case "OK":

                    result.Success = true;
                    break;

                case "MALFORMED":
                    result.Message = (string.Format("Error ({0}: {1}) <br/> {2}", strStatus, strStatusDetail, data.Encode()));
                    break;

                case "INVALID":
                    result.Message = (string.Format("Error ({0}: {1}) <br/> {2}", strStatus, strStatusDetail, data.Encode()));
                    break;

                default:
                    result.Message = (string.Format("Error ({0}: {1})", strStatus, strStatusDetail));
                    break;

            }

            return result;
        }

        /// <summary>
        /// Void an order
        /// </summary>
        /// <param name="orderGuid">Order guid</param>
        /// <returns></returns>
        public VoidTransactionResult VoidTransaction(String orderGuid)
        {
            var result = new VoidTransactionResult();

            var transx = _sagePayServerTransactionService.GetSagePayServerTransactionByVendorTxCode(orderGuid);

            if (transx == null)
            {
                result.Message = String.Format("SagePay Server vendor transaction code {0} does not exist.", orderGuid);
                return result;
            }


            var webClient = new WebClient();
            var voidGuid = Guid.NewGuid();

            var data = new QueryStringNameValueCollection
                           {
                               {"VPSProtocol", SagePayHelper.GetProtocol()},
                               {"TxType", "VOID"},
                               {"Vendor", _sagePayServerPaymentSettings.VendorName},
                               {"VendorTxCode", voidGuid.ToString()},
                               {"VPSTxId", transx.VpsTxId},
                               {"SecurityKey", transx.SecurityKey},
                               {"TxAuthNo", transx.TxAuthNo},
                           };

            var postUrl = SagePayHelper.GetSageSystemUrl(_sagePayServerPaymentSettings.ConnectTo, "void");

            string strResponse;

            try
            {

                var responseData = webClient.UploadValues(postUrl, data);

                strResponse = Encoding.ASCII.GetString(responseData);


            }
            catch (WebException ex)
            {
                result.Message = (String.Format(
                    @"Your server was unable to release this transaction with Sage Pay.
                    Check that you do not have a firewall restricting the POST and 
                    that your server can correctly resolve the address {0}. <br/>
                    The Status Number is: {1}<br/>
                    The Description given is: {2}", postUrl, ex.Status, ex.Message));
                return result;
            }

            if (string.IsNullOrWhiteSpace(strResponse))
            {
                result.Message = (String.Format(
                    @"Your server was unable to register this transaction with Sage Pay.
                    Check that you do not have a firewall restricting the POST and 
                    that your server can correctly resolve the address {0}.", postUrl));
                return result;
            }
            var strStatus = SagePayHelper.FindField("Status", strResponse);
            var strStatusDetail = SagePayHelper.FindField("StatusDetail", strResponse);

            switch (strStatus)
            {
                case "OK":

                    result.Success = true;
                    break;

                case "MALFORMED":
                    result.Message = (string.Format("Error ({0}: {1}) <br/> {2}", strStatus, strStatusDetail, data.Encode()));
                    break;

                case "INVALID":
                    result.Message = (string.Format("Error ({0}: {1}) <br/> {2}", strStatus, strStatusDetail, data.Encode()));
                    break;

                default:
                    result.Message = (string.Format("Error ({0}: {1})", strStatus, strStatusDetail));
                    break;

            }

            return result;
        }

        #endregion
    }

    #region Required classes

    public partial class RegisterTransactionResult
    {
        public string Message { get; set; }
        public string PaymentUrl { get; set; }
        public bool Success { get; set; }
    }

    public partial class ReleaseTransactionResult
    {
        public string Message { get; set; }
        public bool Success { get; set; }
    }

    public partial class RefundTransactionResult
    {
        public string Message { get; set; }
        public bool Success { get; set; }
    }

    public partial class VoidTransactionResult
    {
        public string Message { get; set; }
        public bool Success { get; set; }
    }
    
    #endregion
}
