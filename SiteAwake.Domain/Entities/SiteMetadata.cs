namespace SiteAwake.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("SiteMetadata")]
    public partial class SiteMetadata
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SiteMetadata()
        {
            Communications = new HashSet<Communication>();
        }

        public long Id { get; set; }

        public long AccountId { get; set; }

        [Required]
        [StringLength(512)]
        public string Url { get; set; }

        public short Interval { get; set; }

        public bool AlertsEnabled { get; set; }

        public bool AlertSent { get; set; }

        public bool Processing { get; set; }

        public DateTime? LastWakeUpCall { get; set; }

        public DateTime Created { get; set; }

        public DateTime? Modified { get; set; }

        public virtual Account Account { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Communication> Communications { get; set; }
    }
}
