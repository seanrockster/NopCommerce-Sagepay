using System.Collections.Generic;
using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using System;

namespace Nop.Plugin.Payments.SagePayServer.Models
{
    public class PaymentSagePayServerModel : BaseNopModel
    {
        public PaymentSagePayServerModel()
        {
            Warnings = new List<string>();
        }

        public bool UseIframe { get; set; }
        public string FrameUrl { get; set; }

        public bool IsOnePageCheckout { get; set; }
        public string OrderGuid { get; set; }

        public List<string> Warnings { get; set; }
    }
}