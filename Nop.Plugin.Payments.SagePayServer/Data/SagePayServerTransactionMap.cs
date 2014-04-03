using System.Data.Entity.ModelConfiguration;
using Nop.Plugin.Payments.SagePayServer.Core.Domain;

namespace Nop.Plugin.Payments.SagePayServer.Data
{
    class SagePayServerTransactionMap : EntityTypeConfiguration<SagePayServerTransaction>
    {
        public SagePayServerTransactionMap()
        {
            this.ToTable("SagePayServerTransaction");
            this.HasKey(o => o.Id);

            this.Property(o => o.VpsTxId).IsMaxLength();
            this.Property(o => o.SecurityKey).IsMaxLength();
            this.Property(o => o.NotificationResponse).IsMaxLength();

            this.Property(o => o.VendorTxCode).IsMaxLength();
            this.Property(o => o.VpsSignature).IsMaxLength();
            this.Property(o => o.Status).IsMaxLength();
            this.Property(o => o.StatusDetail).IsMaxLength();
            this.Property(o => o.TxAuthNo).IsMaxLength();
            this.Property(o => o.Avscv2).IsMaxLength();
            this.Property(o => o.AddressResult).IsMaxLength();
            this.Property(o => o.PostCodeResult).IsMaxLength();
            this.Property(o => o.Cv2Result).IsMaxLength();
            this.Property(o => o.GiftAid).IsMaxLength();
            this.Property(o => o.ThreeDSecureStatus).IsMaxLength();
            this.Property(o => o.Cavv).IsMaxLength();
            this.Property(o => o.AddressStatus).IsMaxLength();
            this.Property(o => o.PayerStatus).IsMaxLength();
            this.Property(o => o.CardType).IsMaxLength();
            this.Property(o => o.Last4Digits).IsMaxLength();
            
        }
    }
}
