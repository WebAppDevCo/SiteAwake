using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiteAwake.PaymentService.Infrastructure;

namespace SiteAwake.PaymentServiceProvider.Infrastructure
{
    public class TransactionResponseError : ITransactionResponseError
    {
        public string ErrorCode { get; set; }
        public string ErrorText { get; set; }
    }
}
