using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using RawnaqProject.Models;

namespace RawnaqProject.Data
{
    public class RawnaqProjectContext : DbContext
    {
        public RawnaqProjectContext (DbContextOptions<RawnaqProjectContext> options)
            : base(options)
        {
        }

        public DbSet<RawnaqProject.Models.usersaccounts> usersaccounts { get; set; } = default!;
        public DbSet<RawnaqProject.Models.orderline> orderline { get; set; }
        public DbSet<RawnaqProject.Models.items>? items { get; set; }
        public DbSet<RawnaqProject.Models.orders>? orders { get; set; }
        public DbSet<RawnaqProject.Models.buyitems>? buyitems { get; set; }
        public DbSet<RawnaqProject.Models.customer>? customer { get; set; }

    }
}
