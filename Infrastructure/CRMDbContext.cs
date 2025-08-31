using CRM.Domain.Entities;
using CRMSystem.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CRM.Infrastructure
{
    public class CRMDbContext : IdentityDbContext<ApplicationUser>
    {
        public CRMDbContext(DbContextOptions<CRMDbContext> options) : base(options) { }

        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Service> Services => Set<Service>();
        public DbSet<Invoice> Invoices => Set<Invoice>();
        public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    }

    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
    }

    // 🔹 Migration ve Database Update işlemleri için gerekli Factory
    public class CRMDbContextFactory : IDesignTimeDbContextFactory<CRMDbContext>
    {
        public CRMDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<CRMDbContext>();

            // Buraya kendi PostgreSQL connection string’ini yaz
       

            return new CRMDbContext(optionsBuilder.Options);
        }
    }
}
