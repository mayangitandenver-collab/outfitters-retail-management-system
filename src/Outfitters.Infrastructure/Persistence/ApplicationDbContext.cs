using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Outfitters.Domain.Entities;

namespace Outfitters.Infrastructure.Persistence;

public sealed class ApplicationDbContext
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();
    public DbSet<CashSession> CashSessions => Set<CashSession>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();
    public DbSet<SalePayment> SalePayments => Set<SalePayment>();
    public DbSet<SaleReturn> SaleReturns => Set<SaleReturn>();
    public DbSet<SaleReturnItem> SaleReturnItems => Set<SaleReturnItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Company>(entity =>
        {
            entity.HasIndex(x => x.Code).IsUnique();
            entity.Property(x => x.Code).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
        });

        builder.Entity<Store>(entity =>
        {
            entity.HasIndex(x => new { x.CompanyId, x.Code }).IsUnique();
            entity.Property(x => x.Code).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.HasOne(x => x.Company)
                .WithMany(x => x.Stores)
                .HasForeignKey(x => x.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            entity.HasOne(x => x.Store)
                .WithMany()
                .HasForeignKey(x => x.StoreId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<Category>(entity =>
        {
            entity.HasIndex(x => x.Name);
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.HasOne(x => x.ParentCategory)
                .WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Brand>(entity =>
        {
            entity.HasIndex(x => x.Name).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
        });

        builder.Entity<Product>(entity =>
        {
            entity.HasIndex(x => x.Sku).IsUnique();
            entity.Property(x => x.Sku).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.HasOne(x => x.Category)
                .WithMany(x => x.Products)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Brand)
                .WithMany(x => x.Products)
                .HasForeignKey(x => x.BrandId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<ProductVariant>(entity =>
        {
            entity.HasIndex(x => x.VariantSku).IsUnique();
            entity.HasIndex(x => x.Barcode).IsUnique();
            entity.Property(x => x.VariantSku).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Barcode).HasMaxLength(100).IsRequired();
            entity.Property(x => x.CostPrice).HasPrecision(18, 2);
            entity.Property(x => x.SellingPrice).HasPrecision(18, 2);
            entity.HasOne(x => x.Product)
                .WithMany(x => x.Variants)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<InventoryItem>(entity =>
        {
            entity.HasIndex(x => new { x.StoreId, x.ProductVariantId }).IsUnique();
            entity.Property(x => x.QuantityOnHand).HasPrecision(18, 3);
            entity.Property(x => x.ReservedQuantity).HasPrecision(18, 3);
            entity.Property(x => x.ReorderPoint).HasPrecision(18, 3);
            entity.Property(x => x.MinimumStock).HasPrecision(18, 3);
            entity.Property(x => x.MaximumStock).HasPrecision(18, 3);
            entity.HasOne(x => x.Store)
                .WithMany()
                .HasForeignKey(x => x.StoreId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ProductVariant)
                .WithMany(x => x.InventoryItems)
                .HasForeignKey(x => x.ProductVariantId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.Ignore(x => x.AvailableQuantity);
        });

        builder.Entity<InventoryTransaction>(entity =>
        {
            entity.Property(x => x.QuantityChange).HasPrecision(18, 3);
            entity.Property(x => x.BalanceAfter).HasPrecision(18, 3);
            entity.HasOne(x => x.Store)
                .WithMany()
                .HasForeignKey(x => x.StoreId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ProductVariant)
                .WithMany()
                .HasForeignKey(x => x.ProductVariantId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CreatedByUser)
                .WithMany()
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<CashSession>(entity =>
        {
            entity.Property(x => x.OpeningCash).HasPrecision(18, 2);
            entity.Property(x => x.ClosingCash).HasPrecision(18, 2);
            entity.Property(x => x.ExpectedCash).HasPrecision(18, 2);
            entity.Property(x => x.CashVariance).HasPrecision(18, 2);
            entity.HasOne(x => x.Store)
                .WithMany()
                .HasForeignKey(x => x.StoreId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.OpenedByUser)
                .WithMany()
                .HasForeignKey(x => x.OpenedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ClosedByUser)
                .WithMany()
                .HasForeignKey(x => x.ClosedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<Sale>(entity =>
        {
            entity.HasIndex(x => x.ReceiptNumber).IsUnique();
            entity.Property(x => x.ReceiptNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Subtotal).HasPrecision(18, 2);
            entity.Property(x => x.DiscountTotal).HasPrecision(18, 2);
            entity.Property(x => x.TaxTotal).HasPrecision(18, 2);
            entity.Property(x => x.GrandTotal).HasPrecision(18, 2);
            entity.Property(x => x.AmountPaid).HasPrecision(18, 2);
            entity.Property(x => x.ChangeDue).HasPrecision(18, 2);
            entity.HasOne(x => x.Store)
                .WithMany()
                .HasForeignKey(x => x.StoreId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CashSession)
                .WithMany(x => x.Sales)
                .HasForeignKey(x => x.CashSessionId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CashierUser)
                .WithMany()
                .HasForeignKey(x => x.CashierUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<SaleItem>(entity =>
        {
            entity.Property(x => x.Quantity).HasPrecision(18, 3);
            entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
            entity.Property(x => x.DiscountAmount).HasPrecision(18, 2);
            entity.Property(x => x.TaxAmount).HasPrecision(18, 2);
            entity.Property(x => x.LineTotal).HasPrecision(18, 2);
            entity.Property(x => x.ReturnedQuantity).HasPrecision(18, 3);
            entity.HasOne(x => x.Sale)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.SaleId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.ProductVariant)
                .WithMany()
                .HasForeignKey(x => x.ProductVariantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<SalePayment>(entity =>
        {
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.HasOne(x => x.Sale)
                .WithMany(x => x.Payments)
                .HasForeignKey(x => x.SaleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<SaleReturn>(entity =>
        {
            entity.HasIndex(x => x.ReturnNumber).IsUnique();
            entity.Property(x => x.ReturnNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.RefundAmount).HasPrecision(18, 2);
            entity.HasOne(x => x.Sale)
                .WithMany()
                .HasForeignKey(x => x.SaleId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ProcessedByUser)
                .WithMany()
                .HasForeignKey(x => x.ProcessedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<SaleReturnItem>(entity =>
        {
            entity.Property(x => x.Quantity).HasPrecision(18, 3);
            entity.Property(x => x.RefundAmount).HasPrecision(18, 2);
            entity.HasOne(x => x.SaleReturn)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.SaleReturnId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.SaleItem)
                .WithMany()
                .HasForeignKey(x => x.SaleItemId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
