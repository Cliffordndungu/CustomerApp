using CustomerApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Numerics;

namespace CustomerApp.Data
{
    public class AppDBContext : IdentityDbContext<ApplicationUser>

    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {
        }
      
        public DbSet<Customertenant> Customertenant { get; set; }


    }
}


