using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiteAwake.PaymentService.Infrastructure
{
    public interface ITransactionResponse
    {
        string AccountNumber { get; set; }
        string AccountType { get; set; }
        string AuthCode { get; set; }
        string AvsResultCode { get; set; }
        string CavvResultCode { get; set; }
        string CvvResultCode { get; set; }
        ITransactionResponseError[] Errors { get; set; }
        ITransactionResponseMessage[] Messages { get; set; }
        string RawResponseCode { get; set; }
        string RefTransID { get; set; }
        string ResponseCode { get; set; }
        string TestRequest { get; set; }
        string TransHash { get; set; }
        string TransId { get; set; }
        string SubscriptionId { get; set; }
        string CustomerProfileId { get; set; }
        string CustomerPaymentProfileId { get; set; }
        bool IsSuccess { get; set; }
    }
}
