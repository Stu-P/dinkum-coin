using DinkumCoin.Mining.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DinkumCoin.Mining.Data.Contexts
{
    public class MiningDbContext : DbContext
    {

        public MiningDbContext(DbContextOptions<MiningDbContext> options) : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MineableTransaction>().Property(t => t.Id).ValueGeneratedNever();
        }

        public DbSet<MiningTask> MiningTasks { get; set; }
        public DbSet<MineableTransaction> Transactions { get; set; }
    }
}
