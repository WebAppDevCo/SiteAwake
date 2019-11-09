using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiteAwake.PaymentService.Infrastructure
{
    public interface ITransactionResponseMessage
    {
        string Code { get; set; }
        string Description { get; set; }
    }
}
