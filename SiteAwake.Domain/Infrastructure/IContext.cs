using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using SiteAwake.Domain.Entities;

namespace SiteAwake.Domain.Infrastructure
{
    public interface IContext
    {
        DbSet<Account> Accounts { get; set; }
        DbSet<Communication> Communications { get; set; }
        DbSet<Subscription> Subscriptions { get; set; }
        DbSet<SiteMetadata> SiteMetadatas { get; set; }

        Database Database { get; }

        DbEntityEntry Entry(object entity);

        int SaveChanges();
        
        Task<int> SaveChangesAsync();
    }
}
