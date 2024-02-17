using Microsoft.EntityFrameworkCore;
using tsuKeysAPIProject.DBContext.Models;

namespace tsuKeysAPIProject.DBContext
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<BlackToken> BlackTokens { get; set; }
        public DbSet<Key> Keys { get; set; }
        public DbSet<KeyRequest> KeyRequest { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasIndex(x => x.Email);

            base.OnModelCreating(modelBuilder);
        }
    }
}
