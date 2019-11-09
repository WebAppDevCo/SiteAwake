namespace SiteAwake.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Account")]
    public partial class Account
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Account()
        {
            SiteMetadatas = new HashSet<SiteMetadata>();
            Subscriptions = new HashSet<Subscription>();
        }

        public long Id { get; set; }

        [Required]
        [StringLength(256)]
        public string Email { get; set; }

        public bool Enabled { get; set; }

        public bool Subscribed { get; set; }

        public bool Verified { get; set; }

        public bool Cancelled { get; set; }

        public DateTime Created { get; set; }

        public DateTime? Modified { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SiteMetadata> SiteMetadatas { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Subscription> Subscriptions { get; set; }
    }
}
