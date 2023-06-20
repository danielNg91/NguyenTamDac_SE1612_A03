using BusinessObjects;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace Repository
{
    public partial class FUFlowerBouquetManagementContext : DbContext
    {
        public FUFlowerBouquetManagementContext()
        {
        }

        public FUFlowerBouquetManagementContext(DbContextOptions<FUFlowerBouquetManagementContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<AspNetUser> Customers { get; set; }
        public virtual DbSet<FlowerBouquet> FlowerBouquets { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<OrderDetail> OrderDetails { get; set; }
        public virtual DbSet<Supplier> Suppliers { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
