using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiteAwake.PaymentService.Models
{
    public interface ILineItem
    {
        string Description { get; set; }
        string ItemId { get; set; }
        string Name { get; set; }
        decimal Quantity { get; set; }
        decimal UnitPrice { get; set; }
    }
}
