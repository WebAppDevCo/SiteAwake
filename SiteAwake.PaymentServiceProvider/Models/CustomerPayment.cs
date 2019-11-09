using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiteAwake.PaymentService.Models;

namespace SiteAwake.PaymentServiceProvider.Models
{
    public enum TransactionStatus
    {
        Success = 1,
        Declined = 2,
        Error = 3,
        New = 4,
        Scheduled = 5,
        Cancelled = 6
    }

    public enum SubscriptionStatus
    {
        Inactive = 1,
        Active = 2,
        Cancelled = 3
    }

    public class CustomerPayment : ICustomerPayment
    {
        public string Id { get; set; }
        public string Company { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        //public string State { get; set; }
        public string Zip { get; set; }
        public string CardNumber { get; set; }
        public string ExpirationDate { get; set; }
        public string CardCode { get; set; }
        public decimal Amount { get; set; }
        public IList<ILineItem> LineItems { get; set; }
        public string IpAddress { get; set; }
        public string Email { get; set; }
        public decimal TrialAmount { get; set; }
        public DateTime StartDate { get; set; }
        public short TotalOccurrences { get; set; }
        public short TrialOccurrences { get; set; }
        public string SubscriptionId { get; set; }
        public string CustomerProfileId { get; set; }
        public string CustomerPaymentProfileId { get; set; }
    }
}
