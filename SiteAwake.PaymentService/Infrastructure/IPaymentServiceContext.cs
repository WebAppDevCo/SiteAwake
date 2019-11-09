using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiteAwake.PaymentService.Models;

namespace SiteAwake.PaymentService.Infrastructure
{
    public interface IPaymentServiceContext
    {
        Task<ITransactionResponse> ChargeCreditCard(ICustomerPayment customerPayment);
        Task<ITransactionResponse> ChargeCustomerProfile(ICustomerPayment customerPayment);
        Task<ITransactionResponse> CreateSubscription(ICustomerPayment customerPayment);
        Task<ITransactionResponse> CancelSubscription(string subscriptionId);
        Task<ITransactionResponse> UpdateSubscription(ICustomerPayment customerPayment);
        Task<ITransactionResponse> CreateCustomerProfileFromTransaction(string transactionId);
        Task<ITransactionResponse> UpdateCustomerPaymentProfile(ICustomerPayment customerPayment);
        Task<ITransactionResponse> DeleteCustomerProfile(string customerProfileId);
    }
}
