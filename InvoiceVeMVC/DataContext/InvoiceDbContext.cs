using InvoiceVeMVC.Models;
using Microsoft.EntityFrameworkCore;

namespace InvoiceVeMVC.DataContext
{
    public class InvoiceDbContext : DbContext
    {
        public InvoiceDbContext(DbContextOptions<InvoiceDbContext> options) : base(options)
        {

        }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<InvoiceViewModel> Invoices { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure precision and scale for decimal properties in Contract entity
            modelBuilder.Entity<Contract>()
                .Property(c => c.Amount)
                .HasColumnType("decimal(18,2)");

            // Configure precision and scale for decimal properties in Invoice entity
            modelBuilder.Entity<InvoiceViewModel>()
                .Property(i => i.Amount)
                .HasColumnType("decimal(18,2)");
        }
    }
}
