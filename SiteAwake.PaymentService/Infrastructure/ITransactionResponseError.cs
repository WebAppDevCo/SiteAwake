using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiteAwake.PaymentService.Infrastructure
{
    public interface ITransactionResponseError
    {
        string ErrorCode { get; set; }
        string ErrorText { get; set; }
    }
}
