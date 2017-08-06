namespace Bigetron.Data
{
    using Core.Domain.Articles;
    using Core.Domain.Users;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;

    public class BTRDbContext : IdentityDbContext<User>
    {
        #region Constructor
        public BTRDbContext(DbContextOptions options) : base(options)
        {
        }
        #endregion

        #region Methods
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Article>().HasIndex(a => a.Title).IsUnique();
        }
        #endregion

        #region Properties
        public DbSet<Article> Articles { get; set; }
        #endregion
    }
}