using BusinessObjects;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace Repository {
    public partial class FUFlowerBouquetManagementContext : IdentityDbContext<AspNetUser, IdentityRole<int>, int> {
        public FUFlowerBouquetManagementContext()
        {
        }

        public FUFlowerBouquetManagementContext(DbContextOptions<FUFlowerBouquetManagementContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<AspNetUser> AspNetUsers { get; set; }
        public virtual DbSet<AspNetRole> AspNetRoles { get; set; }
        public virtual DbSet<FlowerBouquet> FlowerBouquets { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<OrderDetail> OrderDetails { get; set; }
        public virtual DbSet<Supplier> Suppliers { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Order>()
                .HasOne(e => e.Customer)
                .WithMany(e => e.Orders)
                .OnDelete(DeleteBehavior.SetNull);
            
            modelBuilder
                .Entity<OrderDetail>()
                .HasOne(e => e.Order)
                .WithMany(e => e.OrderDetails)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder
                .Entity<OrderDetail>()
                .HasOne(e => e.FlowerBouquet)
                .WithMany(e => e.OrderDetails)
                .OnDelete(DeleteBehavior.Cascade);
            
            base.OnModelCreating(modelBuilder);
        }
    }
}
