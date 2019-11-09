using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiteAwake.PaymentService.Infrastructure;

namespace SiteAwake.PaymentServiceProvider.Infrastructure
{
    public class TransactionResponse : ITransactionResponse
    {
        public string AccountNumber { get; set; }
        public string AccountType { get; set; }
        public string AuthCode { get; set; }
        public string AvsResultCode { get; set; }
        public string CavvResultCode { get; set; }
        public string CvvResultCode { get; set; }
        public ITransactionResponseError[] Errors { get; set; }
        public ITransactionResponseMessage[] Messages { get; set; }
        public string RawResponseCode { get; set; }
        public string RefTransID { get; set; }
        public string ResponseCode { get; set; }
        public string TestRequest { get; set; }
        public string TransHash { get; set; }
        public string TransId { get; set; }
        public string SubscriptionId { get; set; }
        public bool IsSuccess { get; set; }
        public string CustomerProfileId { get; set; }
        public string CustomerPaymentProfileId { get; set; }
    }
}
