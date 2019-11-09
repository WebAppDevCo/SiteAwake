namespace SiteAwake.Domain.Entities
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class SiteAwakeDbContext : DbContext
    {
        public SiteAwakeDbContext()
            : base("name=SiteAwakeDbContext")
        {
        }

        public virtual DbSet<Account> Accounts { get; set; }
        public virtual DbSet<Communication> Communications { get; set; }
        public virtual DbSet<SiteMetadata> SiteMetadatas { get; set; }
        public virtual DbSet<Subscription> Subscriptions { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SiteMetadata>()
                .Property(e => e.Url)
                .IsUnicode(false);
        }
    }
}
