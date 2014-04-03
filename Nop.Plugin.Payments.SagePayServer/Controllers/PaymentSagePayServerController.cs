using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Payments.SagePayServer.Core.Domain;
using Nop.Plugin.Payments.SagePayServer.Models;
using Nop.Plugin.Payments.SagePayServer.Services;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Payments;
using Nop.Services.Stores;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

namespace Nop.Plugin.Payments.SagePayServer.Controllers
{
    public class PaymentSagePayServerController : BaseNopPaymentController
    {

        #region Private fields

        private readonly ISagePayServerWorkflowService _sagePayServerWorkflowService;
        private readonly ISettingService _settingService;
        private readonly ISagePayServerTransactionService _sagePayServerTransactionService;
        private readonly IMobileDeviceHelper _mobileDeviceHelper;

        private readonly IStoreService _storeService;
        private readonly IWorkContext _workContext;
        private readonly OrderSettings _orderSettings;
        private readonly HttpContextBase _httpContext;

        #endregion 
        
        #region Constructor

        public PaymentSagePayServerController(ISettingService settingService, ISagePayServerTransactionService sagePayServerTransactionService,
            IMobileDeviceHelper mobileDeviceHelper, ISagePayServerWorkflowService sagePayServerWorkflowService, 
            IStoreService storeService, IWorkContext workContext,
            OrderSettings orderSettings, HttpContextBase httpContext)
        {
            this._settingService = settingService;
            this._sagePayServerTransactionService = sagePayServerTransactionService;
            this._storeService = storeService;
            this._orderSettings = orderSettings;
            this._httpContext = httpContext;
            this._mobileDeviceHelper = mobileDeviceHelper;
            this._sagePayServerWorkflowService = sagePayServerWorkflowService;
            this._workContext = workContext;
        }

        #endregion 

        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var sagePayServerPaymentSettings = _settingService.LoadSetting<SagePayServerPaymentSettings>(storeScope);

            var model = new ConfigurationModel
                            {
                                ConnectToId = Convert.ToInt32(sagePayServerPaymentSettings.ConnectTo),
                                TransactTypeId = Convert.ToInt32(sagePayServerPaymentSettings.TransactType),
                                ProfileId = Convert.ToInt32(sagePayServerPaymentSettings.Profile),
                                VendorName = sagePayServerPaymentSettings.VendorName,
                                PartnerId = sagePayServerPaymentSettings.PartnerId,
                                AdditionalFee = sagePayServerPaymentSettings.AdditionalFee,
                            };

            model.AvailableTransactTypes = sagePayServerPaymentSettings.TransactType.ToSelectList();
            model.AvailableConnectTos = sagePayServerPaymentSettings.ConnectTo.ToSelectList();
            model.AvailableProfiles = sagePayServerPaymentSettings.Profile.ToSelectList();
            
            model.ActiveStoreScopeConfiguration = storeScope;
            if (storeScope > 0)
            {
                model.ConnectToId_OverrideForStore = _settingService.SettingExists(sagePayServerPaymentSettings, x => x.ConnectTo, storeScope);
                model.PartnerId_OverrideForStore = _settingService.SettingExists(sagePayServerPaymentSettings, x => x.PartnerId, storeScope);
                model.TransactTypeId_OverrideForStore = _settingService.SettingExists(sagePayServerPaymentSettings, x => x.TransactType, storeScope);
                model.VendorName_OverrideForStore = _settingService.SettingExists(sagePayServerPaymentSettings, x => x.VendorName, storeScope);
                model.ProfileId_OverrideForStore = _settingService.SettingExists(sagePayServerPaymentSettings, x => x.Profile, storeScope);
                model.AdditionalFee_OverrideForStore = _settingService.SettingExists(sagePayServerPaymentSettings, x => x.AdditionalFee, storeScope);
            }

            return View("Nop.Plugin.Payments.SagePayServer.Views.PaymentSagePayServer.Configure", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var sagePayServerPaymentSettings = _settingService.LoadSetting<SagePayServerPaymentSettings>(storeScope);

            //save settings
            sagePayServerPaymentSettings.ConnectTo = (ConnectToValues)model.ConnectToId;
            sagePayServerPaymentSettings.TransactType = (TransactTypeValues)model.TransactTypeId;
            sagePayServerPaymentSettings.Profile = (ProfileValues)model.ProfileId;
            sagePayServerPaymentSettings.VendorName = model.VendorName;
            sagePayServerPaymentSettings.PartnerId = model.PartnerId;
            sagePayServerPaymentSettings.AdditionalFee = model.AdditionalFee;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            if (model.ConnectToId_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(sagePayServerPaymentSettings, x => x.ConnectTo, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(sagePayServerPaymentSettings, x => x.ConnectTo, storeScope);

            if (model.TransactTypeId_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(sagePayServerPaymentSettings, x => x.TransactType, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(sagePayServerPaymentSettings, x => x.TransactType, storeScope);

            if (model.ProfileId_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(sagePayServerPaymentSettings, x => x.Profile, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(sagePayServerPaymentSettings, x => x.Profile, storeScope);

            if (model.VendorName_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(sagePayServerPaymentSettings, x => x.VendorName, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(sagePayServerPaymentSettings, x => x.VendorName, storeScope);

            if (model.PartnerId_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(sagePayServerPaymentSettings, x => x.PartnerId, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(sagePayServerPaymentSettings, x => x.PartnerId, storeScope);

            if (model.AdditionalFee_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(sagePayServerPaymentSettings, x => x.AdditionalFee, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(sagePayServerPaymentSettings, x => x.AdditionalFee, storeScope);

            //now clear settings cache
            _settingService.ClearCache();

            return Configure();
        }

        [ChildActionOnly]
        public ActionResult PaymentInfo()
        {
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var sagePayServerPaymentSettings = _settingService.LoadSetting<SagePayServerPaymentSettings>(storeScope);

            var model = new PaymentSagePayServerModel
                            {
                                IsOnePageCheckout = UseOnePageCheckout()
                            };

            var result = _sagePayServerWorkflowService.RegisterTransaction();

            if (result.Success == false)
            {
                model.Warnings.Add(result.Message);
                return View("Nop.Plugin.Payments.SagePayServer.Views.PaymentSagePayServer.PaymentInfo", model);
            }

            if (sagePayServerPaymentSettings.Profile == ProfileValues.Low || model.IsOnePageCheckout)
            {
                //Iframe
                model.FrameUrl = result.PaymentUrl;

                return View("Nop.Plugin.Payments.SagePayServer.Views.PaymentSagePayServer.PaymentInfo", model);
            } 
            else
            {
                _httpContext.Response.Redirect(result.PaymentUrl);
                _httpContext.Response.End();
                return null;
            }


        }
        
        public ActionResult NotificationPage()
        {
            var strTxAuthNo = Request.Params["TxAuthNo"];
            var strAvscv2 = Request.Params["AVSCV2"];
            var strAddressResult = Request.Params["AddressResult"];
            var strPostCodeResult = Request.Params["PostCodeResult"];
            var strCv2Result = Request.Params["CV2Result"];
            var strGiftAid = Request.Params["GiftAid"];
            var str3DSecureStatus = Request.Params["3DSecureStatus"];
            var strCavv = Request.Params["CAVV"];
            var strAddressStatus = Request.Params["AddressStatus"];
            var strPayerStatus = Request.Params["PayerStatus"];
            var strCardType = Request.Params["CardType"];
            var strLast4Digits = Request.Params["Last4Digits"];

            var strVpsTxId = Request.Params["VPSTxId"];
            var strVpsSignature = Request.Params["VPSSignature"];
            var strStatus = Request.Params["Status"];
            var strStatusDetail = Request.Params["StatusDetail"];
            var strVendorTxCode = Request.Params["VendorTxCode"];


            var message = _sagePayServerWorkflowService.ValidateTransaction(strTxAuthNo, strAvscv2, strAddressResult, strPostCodeResult, strCv2Result, strGiftAid, str3DSecureStatus,
                strCavv, strAddressStatus, strPayerStatus, strCardType, strLast4Digits, strVpsTxId, strVpsSignature, strStatus, strStatusDetail, strVendorTxCode);

            return Content(message);
        }

        /// <summary>
        /// Action performed right after Sage pay Redirects from the Notification page. **It does not validate the session (if inside an iframe)
        /// </summary>
        /// <returns></returns>
        public ActionResult ResponsePage()
        {
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var sagePayServerPaymentSettings = _settingService.LoadSetting<SagePayServerPaymentSettings>(storeScope);

            var model = new PaymentSagePayServerModel();

            var strOrderGuid = Request.QueryString["uid"];

            var transx = _sagePayServerTransactionService.GetSagePayServerTransactionByVendorTxCode(strOrderGuid);

            if (transx == null)
            {
                model.Warnings.Add(String.Format("SagePay Server vendor transaction code {0} does not exist.", strOrderGuid));
                return View("Nop.Plugin.Payments.SagePayServer.Views.PaymentSagePayServer.ResponsePage", model);
            }

            model.IsOnePageCheckout = UseOnePageCheckout();

            model.UseIframe = (sagePayServerPaymentSettings.Profile == ProfileValues.Low || model.IsOnePageCheckout);

            if ((transx.Status == "OK") || (transx.Status == "AUTHENTICATED") || (transx.Status == "REGISTERED"))
            {
                model.OrderGuid = transx.VendorTxCode;
            }
            else
            {
                model.Warnings.Add(transx.StatusDetail);
            }

            return View("Nop.Plugin.Payments.SagePayServer.Views.PaymentSagePayServer.ResponsePage", model);
        }

        [NonAction]
        protected bool UseOnePageCheckout()
        {
            var useMobileDevice = _mobileDeviceHelper.IsMobileDevice(_httpContext)
                && _mobileDeviceHelper.MobileDevicesSupported()
                && !_mobileDeviceHelper.CustomerDontUseMobileVersion();

            //mobile version doesn't support one-page checkout
            if (useMobileDevice)
                return false;

            //check the appropriate setting
            return _orderSettings.OnePageCheckoutEnabled;
        }

        /// <summary>
        /// Validates the iframe (not form) to see if the Notification response was valid
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [NonAction]
        public override IList<string> ValidatePaymentForm(FormCollection form)
        {
            var warnings = new List<string>();

            var strOrderGuid = _httpContext.Request.Form["OrderGuid"];

            if (String.IsNullOrWhiteSpace(strOrderGuid))
            {
                warnings.Add("OrderGuid does not exist or it is empty.");
                return warnings;
            }

            Guid orderGuid;

            if (Guid.TryParse(strOrderGuid, out orderGuid) == false)
            {
                warnings.Add(String.Format("OrderGuid {0} is invalid", strOrderGuid));
            }

            return warnings;
        }

        [NonAction]
        public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
        {
            Guid orderGuid;

            Guid.TryParse(_httpContext.Request.Form["orderGuid"], out orderGuid);

            var paymentInfo = new ProcessPaymentRequest
                                  {
                                      OrderGuid = orderGuid
                                  };

            return paymentInfo;
        }




    }
}