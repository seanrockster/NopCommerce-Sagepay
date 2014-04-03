using System;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Nop.Plugin.Payments.SagePayServer.Core.Domain;

namespace Nop.Plugin.Payments.SagePayServer.Core
{
    /// <summary>
    /// Represents paypal helper
    /// </summary>
    public class SagePayHelper
    {


        /// <summary>
        /// Gets Sage Pay URL
        /// </summary>
        /// <returns></returns>
        public static string GetSageSystemUrl(ConnectToValues connectTo, string strType)
        {
            var strSystemUrl = String.Empty;

            if (connectTo == ConnectToValues.Live)
            {
                switch (strType)
                {
                    case "abort":
                        strSystemUrl = "https://live.sagepay.com/gateway/service/abort.vsp";
                        break;
                    case "authorise":
                        strSystemUrl = "https://live.sagepay.com/gateway/service/authorise.vsp";
                        break;
                    case "cancel":
                        strSystemUrl = "https://live.sagepay.com/gateway/service/cancel.vsp";
                        break;
                    case "purchase":
                        strSystemUrl = "https://live.sagepay.com/gateway/service/vspserver-register.vsp";
                        break;
                    case "refund":
                        strSystemUrl = "https://live.sagepay.com/gateway/service/refund.vsp";
                        break;
                    case "release":
                        strSystemUrl = "https://live.sagepay.com/gateway/service/release.vsp";
                        break;
                    case "repeat":
                        strSystemUrl = "https://live.sagepay.com/gateway/service/repeat.vsp";
                        break;
                    case "void":
                        strSystemUrl = "https://live.sagepay.com/gateway/service/void.vsp";
                        break;
                    case "3dcallback":
                        strSystemUrl = "https://live.sagepay.com/gateway/service/direct3dcallback.vsp";
                        break;
                    case "showpost":
                        strSystemUrl = "https://live.sagepay.com/showpost/showpost.asp";
                        break;
                    default:
                        break;
                }
            }
            else if (connectTo == ConnectToValues.Test)
            {
                switch (strType)
                {
                    case "abort":
                        strSystemUrl = "https://test.sagepay.com/gateway/service/abort.vsp";
                        break;
                    case "authorise":
                        strSystemUrl = "https://test.sagepay.com/gateway/service/authorise.vsp";
                        break;
                    case "cancel":
                        strSystemUrl = "https://test.sagepay.com/gateway/service/cancel.vsp";
                        break;
                    case "purchase":
                        strSystemUrl = "https://test.sagepay.com/gateway/service/vspserver-register.vsp";
                        break;
                    case "refund":
                        strSystemUrl = "https://test.sagepay.com/gateway/service/refund.vsp";
                        break;
                    case "release":
                        strSystemUrl = "https://test.sagepay.com/gateway/service/release.vsp";
                        break;
                    case "repeat":
                        strSystemUrl = "https://test.sagepay.com/gateway/service/repeat.vsp";
                        break;
                    case "void":
                        strSystemUrl = "https://test.sagepay.com/gateway/service/void.vsp";
                        break;
                    case "3dcallback":
                        strSystemUrl = "https://test.sagepay.com/gateway/service/direct3dcallback.vsp";
                        break;
                    case "showpost":
                        strSystemUrl = "https://test.sagepay.com/showpost/showpost.asp";
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (strType)
                {
                    case "abort":
                        strSystemUrl = "https://test.sagepay.com/simulator/vspserverGateway.asp?Service=VendorAbortTx";
                        break;
                    case "authorise":
                        strSystemUrl = "https://test.sagepay.com/simulator/vspserverGateway.asp?Service=VendorAuthoriseTx";
                        break;
                    case "cancel":
                        strSystemUrl = "https://test.sagepay.com/simulator/vspserverGateway.asp?Service=VendorCancelTx";
                        break;
                    case "purchase":
                        strSystemUrl = "https://test.sagepay.com/simulator/VSPServerGateway.asp?Service=VendorRegisterTx";
                        break;
                    case "refund":
                        strSystemUrl = "https://test.sagepay.com/simulator/vspserverGateway.asp?Service=VendorRefundTx";
                        break;
                    case "release":
                        strSystemUrl = "https://test.sagepay.com/simulator/vspserverGateway.asp?Service=VendorReleaseTx";
                        break;
                    case "repeat":
                        strSystemUrl = "https://test.sagepay.com/simulator/vspserverGateway.asp?Service=VendorRepeatTx";
                        break;
                    case "void":
                        strSystemUrl = "https://test.sagepay.com/simulator/vspserverGateway.asp?Service=VendorVoidTx";
                        break;
                    case "3dcallback":
                        strSystemUrl = "https://test.sagepay.com/simulator/vspserverCallback.asp";
                        break;
                    case "showpost":
                        strSystemUrl = "https://test.sagepay.com/showpost/showpost.asp";
                        break;
                    default:
                        break;
                }
            }
            return strSystemUrl;
        }


        public static string GetProtocol()
        {
            return "2.23";
        }

        public static string UrlEncode(string strString)
        {
            return HttpUtility.UrlEncode(strString, Encoding.GetEncoding("ISO-8859-15"));
        }


        public static string FindField(string field, string strResponse)
        {
            var delimiters = new string[1];
            delimiters[0] = Environment.NewLine;
            var fieldList = strResponse.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

            foreach (var s in fieldList)
            {
                if (s.StartsWith(field + "=", StringComparison.CurrentCultureIgnoreCase))
                {
                    return s.Substring(field.Length + 1);
                }
            }
            return "";
        }

        public static string HashMd5(string message)
        {
            var algorithm = MD5.Create();
            var data = algorithm.ComputeHash(Encoding.UTF8.GetBytes(message));
            var sh1 = "";
            for (var i = 0; i < data.Length; i++)
            {
                sh1 += data[i].ToString("x2").ToUpperInvariant();
            }
            return sh1;
        }
    }
}

