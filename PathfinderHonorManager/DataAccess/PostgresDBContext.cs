using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Proxies;
using PathfinderHonorManager.Models;


namespace PathfinderHonorManager.DataAccess
{
    public class PostgresContext : DbContext
    {
        public DbSet<Pathfinder> Pathfinders { get; set; }

        public DbSet<Honor> Honors { get; set; }

        public DbSet<PathfinderHonor> PathfinderHonors { get; set; }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //    => optionsBuilder.UseNpgsql("Host=localhost;Database=pathfinders;Username=dbuser;Password=dbpassword");
        public PostgresContext(DbContextOptions<PostgresContext> options)
        : base(options)
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseLazyLoadingProxies();
    }

}