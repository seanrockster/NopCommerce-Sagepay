using System;

namespace Nop.Plugin.Payments.SagePayServer.Services
{
    public interface ISagePayServerWorkflowService
    {
        /// <summary>
        /// Register a new transaction with Sagepay before the iframe shows. In fact, this method should give us the URL for the iframe
        /// </summary>
        /// <returns></returns>
        RegisterTransactionResult RegisterTransaction();

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
        string ValidateTransaction(string strTxAuthNo,string strAvscv2,
                                                   string strAddressResult,string strPostCodeResult,string strCv2Result,string strGiftAid,string str3DSecureStatus,string strCavv,
                                                   string strAddressStatus,string strPayerStatus,string strCardType,string strLast4Digits,string strVpsTxId,string strVpsSignature,
                                                   string strStatus,string strStatusDetail,string strVendorTxCode);

        /// <summary>
        /// Release an order that was deferred
        /// </summary>
        /// <param name="orderGuid">Order guid</param>
        /// <param name="orderTotal">Order total</param>
        /// <returns></returns>
        ReleaseTransactionResult ReleaseTransaction(String orderGuid, decimal orderTotal);

        /// <summary>
        /// Refund an order that has been paid or released
        /// </summary>
        /// <param name="orderGuid">Order guid</param>
        /// <param name="orderTotal">Order total</param>
        /// <param name="currencyCode">Currency code</param>
        /// <returns></returns>
        RefundTransactionResult RefundTransaction(String orderGuid, decimal orderTotal, string currencyCode);

        /// <summary>
        /// Void an order
        /// </summary>
        /// <param name="orderGuid">Order guid</param>
        /// <returns></returns>
        VoidTransactionResult VoidTransaction(String orderGuid);
    }
}