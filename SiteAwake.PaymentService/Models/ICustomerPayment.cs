using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiteAwake.PaymentService.Models
{
    public interface ICustomerPayment
    {
        string Id { get; set; }
        string Company { get; set; }
        string FirstName { get; set; }
        string LastName { get; set; }
        string Address { get; set; }
        string City { get; set; }
        //string State { get; set; }
        string Zip { get; set; }
        string CardNumber { get; set; }
        string ExpirationDate { get; set; }
        string CardCode { get; set; }
        decimal Amount { get; set; }
        decimal TrialAmount { get; set; }
        IList<ILineItem> LineItems { get; set; }       
        string IpAddress { get; set; }
        string Email { get; set; }
        DateTime StartDate { get; set; }
        short TotalOccurrences { get; set; }
        short TrialOccurrences { get; set; }
        string SubscriptionId { get; set; }
        string CustomerProfileId { get; set; }
        string CustomerPaymentProfileId { get; set; }
    }
}
