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
        }
        #endregion

        #region Properties
        public DbSet<Article> Articles { get; set; }
        #endregion
    }
}