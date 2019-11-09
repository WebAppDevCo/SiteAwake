namespace SiteAwake.DomainProvider.Infrastructure
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using SiteAwake.Domain.Entities;
    using SiteAwake.Domain.Infrastructure;

    public partial class SiteAwakeDbContext : DbContext, IContext
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
