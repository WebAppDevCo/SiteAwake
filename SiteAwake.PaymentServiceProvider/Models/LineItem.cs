using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiteAwake.PaymentService.Models;

namespace SiteAwake.PaymentServiceProvider.Models
{
    public class LineItem : ILineItem
    {
        public string Description { get; set; }
        public string ItemId { get; set; }
        public string Name { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
