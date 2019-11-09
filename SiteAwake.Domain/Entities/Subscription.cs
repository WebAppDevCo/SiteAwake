namespace SiteAwake.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Subscription")]
    public partial class Subscription
    {
        public long Id { get; set; }

        public long AccountId { get; set; }

        [Required]
        [StringLength(256)]
        public string TransactionId { get; set; }

        public short TransactionCode { get; set; }

        public short TransactionStatus { get; set; }

        [Required]
        [StringLength(64)]
        public string AuthCode { get; set; }

        [Required]
        [StringLength(256)]
        public string SubscriptionId { get; set; }

        public short SubscriptionStatus { get; set; }

        [Required]
        [StringLength(64)]
        public string CustomerProfileId { get; set; }

        [Required]
        [StringLength(64)]
        public string CustomerPaymentProfileId { get; set; }

        public DateTime Created { get; set; }

        public virtual Account Account { get; set; }
    }
}
