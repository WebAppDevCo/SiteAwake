namespace SiteAwake.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Communication")]
    public partial class Communication
    {
        public long Id { get; set; }

        public long SiteMetadataId { get; set; }

        [Required]
        [StringLength(256)]
        public string Status { get; set; }

        [StringLength(512)]
        public string Message { get; set; }

        public long MillisecondsElapsed { get; set; }

        public DateTime WakeUpCall { get; set; }

        public DateTime Created { get; set; }

        public virtual SiteMetadata SiteMetadata { get; set; }
    }
}
