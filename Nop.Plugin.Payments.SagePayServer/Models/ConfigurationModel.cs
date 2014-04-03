using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using System.Web.Mvc;

namespace Nop.Plugin.Payments.SagePayServer.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        public int ConnectToId { get; set; }
        public bool ConnectToId_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Payments.SagePayServer.Fields.ConnectTo")]
        public SelectList AvailableConnectTos { get; set; }

        public int TransactTypeId { get; set; }
        public bool TransactTypeId_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Payments.SagePayServer.Fields.TransactType")]
        public SelectList AvailableTransactTypes { get; set; }

        public int ProfileId { get; set; }
        public bool ProfileId_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Payments.SagePayServer.Fields.Profile")]
        public SelectList AvailableProfiles { get; set; }

        [NopResourceDisplayName("Plugins.Payments.SagePayServer.Fields.VendorName")]
        public string VendorName { get; set; }
        public bool VendorName_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.SagePayServer.Fields.PartnerId")]
        public string PartnerId { get; set; }
        public bool PartnerId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.SagePayServer.Fields.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }
    }
}