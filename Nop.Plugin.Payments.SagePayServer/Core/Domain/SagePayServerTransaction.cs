using System;
using System.Text;
using Nop.Core;

namespace Nop.Plugin.Payments.SagePayServer.Core.Domain
{
    public partial class SagePayServerTransaction : BaseEntity
    {
        public virtual string VpsTxId { get; set; }
        public virtual string SecurityKey { get; set; }
        public virtual string NotificationResponse { get; set; }

        public virtual string VendorTxCode { get; set; }
        public virtual string VpsSignature { get; set; }
        public virtual string Status { get; set; }
        public virtual string StatusDetail { get; set; }
        public virtual string TxAuthNo { get; set; }
        public virtual string Avscv2 { get; set; }
        public virtual string AddressResult { get; set; }
        public virtual string PostCodeResult { get; set; }
        public virtual string Cv2Result { get; set; }
        public virtual string GiftAid { get; set; }
        public virtual string ThreeDSecureStatus { get; set; }
        public virtual string Cavv { get; set; }
        public virtual string AddressStatus { get; set; }
        public virtual string PayerStatus { get; set; }
        public virtual string CardType { get; set; }
        public virtual string Last4Digits { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance creation
        /// </summary>
        public virtual DateTime CreatedOnUtc { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("SagePayServer Notification Page Results:");
            sb.AppendLine("Date Created: " + CreatedOnUtc);
            sb.AppendLine("VPSTxId: " + VpsTxId);
            sb.AppendLine("VendorTxCode: " + VendorTxCode);
            sb.AppendLine("VPSSignature: " + VpsSignature);
            sb.AppendLine("Status: " + Status);
            sb.AppendLine("StatusDetail: " + StatusDetail);

            sb.AppendLine("TxAuthNo: " + TxAuthNo);
            sb.AppendLine("AVSCV2: " + Avscv2);
            sb.AppendLine("AddressResult: " + AddressResult);
            sb.AppendLine("PostCodeResult: " + PostCodeResult);
            sb.AppendLine("CV2Result: " + Cv2Result);
            sb.AppendLine("GiftAid: " + GiftAid);
            sb.AppendLine("3DSecureStatus: " + ThreeDSecureStatus);
            sb.AppendLine("CAVV: " + Cavv);
            sb.AppendLine("AddressStatus: " + AddressStatus);
            sb.AppendLine("PayerStatus: " + PayerStatus);
            sb.AppendLine("CardType: " + CardType);
            sb.AppendLine("Last4Digits: " + Last4Digits);

            return sb.ToString();
        }
    }
}
