using CatalogService.DAL.Classes.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.DAL.Classes.Data
{
    public class CatalogBDContext : DbContext
    {

        public CatalogBDContext(DbContextOptions<CatalogBDContext> options) : base(options)
        {
            this.ChangeTracker.LazyLoadingEnabled = false;
            // Only set command timeout for relational providers (InMemory will throw)
            if (this.Database.IsRelational())
            {
                this.Database.SetCommandTimeout(50);
            }
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>()
                .HasMany(c => c.Products)
                .WithOne(p => p.Category)
                .HasForeignKey(p => p.CategoryId);

            modelBuilder.Entity<Category>()
                .HasOne(c => c.ParentCategory)
                .WithMany()
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);
        }
    }
}
