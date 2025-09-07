using CRMSystem.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CRMSystem.Infrastructure
{
    public class CRMDbContext : IdentityDbContext<ApplicationUser>
    {
        public CRMDbContext(DbContextOptions<CRMDbContext> options) : base(options) { }

        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Service> Services => Set<Service>();
        public DbSet<Invoice> Invoices => Set<Invoice>();
        public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Soft Delete Global Query Filter
            builder.Entity<Customer>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<Service>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<Invoice>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<InvoiceItem>().HasQueryFilter(e => !e.IsDeleted);

            // Customer Configuration
            builder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.TaxOffice).HasMaxLength(100);
                entity.Property(e => e.TaxNumber).HasMaxLength(20);
                entity.Property(e => e.Address).HasMaxLength(500);
                entity.Property(e => e.Description).HasMaxLength(1000);
                
                // Unique constraints
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.TaxNumber).IsUnique().HasFilter("[TaxNumber] IS NOT NULL AND [TaxNumber] != ''");
                
                // Relationships
                entity.HasMany(e => e.Invoices)
                      .WithOne(e => e.Customer)
                      .HasForeignKey(e => e.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Service Configuration
            builder.Entity<Service>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Description).HasMaxLength(500);
                
                // Unique constraint
                entity.HasIndex(e => e.Name).IsUnique();
                
                // Relationships
                entity.HasMany(e => e.InvoiceItems)
                      .WithOne(e => e.Service)
                      .HasForeignKey(e => e.ServiceId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Invoice Configuration
            builder.Entity<Invoice>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CustomerId).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Status).IsRequired();
                
                // Relationships
                entity.HasOne(e => e.Customer)
                      .WithMany(e => e.Invoices)
                      .HasForeignKey(e => e.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict);
                      
                entity.HasMany(e => e.Items)
                      .WithOne(e => e.Invoice)
                      .HasForeignKey(e => e.InvoiceId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // InvoiceItem Configuration
            builder.Entity<InvoiceItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.InvoiceId).IsRequired();
                entity.Property(e => e.ServiceId).IsRequired();
                entity.Property(e => e.RenewalCycle).IsRequired();
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Quantity).IsRequired();
                entity.Property(e => e.VAT).HasColumnType("decimal(5,2)");
                entity.Property(e => e.StartDate).IsRequired();
                entity.Property(e => e.DueDate).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(1000);
                
                // Relationships
                entity.HasOne(e => e.Invoice)
                      .WithMany(e => e.Items)
                      .HasForeignKey(e => e.InvoiceId)
                      .OnDelete(DeleteBehavior.Cascade);
                      
                entity.HasOne(e => e.Service)
                      .WithMany(e => e.InvoiceItems)
                      .HasForeignKey(e => e.ServiceId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ApplicationUser Configuration
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
            });
        }
    }

    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
    }
}
