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
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();
    public DbSet<GoodsReceipt> GoodsReceipts => Set<GoodsReceipt>();
    public DbSet<GoodsReceiptItem> GoodsReceiptItems => Set<GoodsReceiptItem>();
    public DbSet<SupplierReturn> SupplierReturns => Set<SupplierReturn>();
    public DbSet<SupplierReturnItem> SupplierReturnItems => Set<SupplierReturnItem>();
    public DbSet<StockTransfer> StockTransfers => Set<StockTransfer>();
    public DbSet<StockTransferItem> StockTransferItems => Set<StockTransferItem>();
    public DbSet<StockTransferReceipt> StockTransferReceipts => Set<StockTransferReceipt>();
    public DbSet<StockTransferReceiptItem> StockTransferReceiptItems => Set<StockTransferReceiptItem>();

                  public DbSet<CustomerTier> CustomerTiers => Set<CustomerTier>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<LoyaltyTransaction> LoyaltyTransactions => Set<LoyaltyTransaction>();
    public DbSet<CustomerVoucher> CustomerVouchers => Set<CustomerVoucher>();
    public DbSet<CustomerFavoriteProduct> CustomerFavoriteProducts => Set<CustomerFavoriteProduct>();

              public DbSet<EmployeeProfile> EmployeeProfiles => Set<EmployeeProfile>();
    public DbSet<EmployeeStoreAssignment> EmployeeStoreAssignments => Set<EmployeeStoreAssignment>();
    public DbSet<EmployeeAttendance> EmployeeAttendanceRecords => Set<EmployeeAttendance>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

              public DbSet<GeneralLedgerAccount> GeneralLedgerAccounts => Set<GeneralLedgerAccount>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<JournalEntryLine> JournalEntryLines => Set<JournalEntryLine>();
    public DbSet<AccountsPayable> AccountsPayables => Set<AccountsPayable>();
    public DbSet<AccountsReceivable> AccountsReceivables => Set<AccountsReceivable>();
    public DbSet<ExpenseRecord> ExpenseRecords => Set<ExpenseRecord>();

              public DbSet<NotificationMessage> NotificationMessages => Set<NotificationMessage>();
    public DbSet<IntegrationSetting> IntegrationSettings => Set<IntegrationSetting>();
    public DbSet<ReceiptPrintJob> ReceiptPrintJobs => Set<ReceiptPrintJob>();
    public DbSet<BarcodeAlias> BarcodeAliases => Set<BarcodeAlias>();

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
            entity.HasOne(x => x.Company).WithMany(x => x.Stores)
                .HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            entity.HasOne(x => x.Store).WithMany()
                .HasForeignKey(x => x.StoreId).OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<Category>(entity =>
        {
            entity.HasIndex(x => x.Name);
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.HasOne(x => x.ParentCategory).WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentCategoryId).OnDelete(DeleteBehavior.Restrict);
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
            entity.HasOne(x => x.Category).WithMany(x => x.Products)
                .HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Brand).WithMany(x => x.Products)
                .HasForeignKey(x => x.BrandId).OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<ProductVariant>(entity =>
        {
            entity.HasIndex(x => x.VariantSku).IsUnique();
            entity.HasIndex(x => x.Barcode).IsUnique();
            entity.Property(x => x.VariantSku).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Barcode).HasMaxLength(100).IsRequired();
            entity.Property(x => x.CostPrice).HasPrecision(18, 2);
            entity.Property(x => x.SellingPrice).HasPrecision(18, 2);
            entity.HasOne(x => x.Product).WithMany(x => x.Variants)
                .HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<InventoryItem>(entity =>
        {
            entity.HasIndex(x => new { x.StoreId, x.ProductVariantId }).IsUnique();
            entity.Property(x => x.QuantityOnHand).HasPrecision(18, 3);
            entity.Property(x => x.ReservedQuantity).HasPrecision(18, 3);
            entity.Property(x => x.ReorderPoint).HasPrecision(18, 3);
            entity.Property(x => x.MinimumStock).HasPrecision(18, 3);
            entity.Property(x => x.MaximumStock).HasPrecision(18, 3);
            entity.HasOne(x => x.Store).WithMany()
                .HasForeignKey(x => x.StoreId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ProductVariant).WithMany(x => x.InventoryItems)
                .HasForeignKey(x => x.ProductVariantId).OnDelete(DeleteBehavior.Restrict);
            entity.Ignore(x => x.AvailableQuantity);
        });

        builder.Entity<InventoryTransaction>(entity =>
        {
            entity.Property(x => x.QuantityChange).HasPrecision(18, 3);
            entity.Property(x => x.BalanceAfter).HasPrecision(18, 3);
            entity.HasOne(x => x.Store).WithMany()
                .HasForeignKey(x => x.StoreId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ProductVariant).WithMany()
                .HasForeignKey(x => x.ProductVariantId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CreatedByUser).WithMany()
                .HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<CashSession>(entity =>
        {
            entity.Property(x => x.OpeningCash).HasPrecision(18, 2);
            entity.Property(x => x.ClosingCash).HasPrecision(18, 2);
            entity.Property(x => x.ExpectedCash).HasPrecision(18, 2);
            entity.Property(x => x.CashVariance).HasPrecision(18, 2);
            entity.HasOne(x => x.Store).WithMany()
                .HasForeignKey(x => x.StoreId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.OpenedByUser).WithMany()
                .HasForeignKey(x => x.OpenedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ClosedByUser).WithMany()
                .HasForeignKey(x => x.ClosedByUserId).OnDelete(DeleteBehavior.SetNull);
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
            entity.HasOne(x => x.Store).WithMany()
                .HasForeignKey(x => x.StoreId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CashSession).WithMany(x => x.Sales)
                .HasForeignKey(x => x.CashSessionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CashierUser).WithMany()
                .HasForeignKey(x => x.CashierUserId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<SaleItem>(entity =>
        {
            entity.Property(x => x.Quantity).HasPrecision(18, 3);
            entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
            entity.Property(x => x.DiscountAmount).HasPrecision(18, 2);
            entity.Property(x => x.TaxAmount).HasPrecision(18, 2);
            entity.Property(x => x.LineTotal).HasPrecision(18, 2);
            entity.Property(x => x.ReturnedQuantity).HasPrecision(18, 3);
            entity.HasOne(x => x.Sale).WithMany(x => x.Items)
                .HasForeignKey(x => x.SaleId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.ProductVariant).WithMany()
                .HasForeignKey(x => x.ProductVariantId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<SalePayment>(entity =>
        {
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.HasOne(x => x.Sale).WithMany(x => x.Payments)
                .HasForeignKey(x => x.SaleId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<SaleReturn>(entity =>
        {
            entity.HasIndex(x => x.ReturnNumber).IsUnique();
            entity.Property(x => x.ReturnNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.RefundAmount).HasPrecision(18, 2);
            entity.HasOne(x => x.Sale).WithMany()
                .HasForeignKey(x => x.SaleId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ProcessedByUser).WithMany()
                .HasForeignKey(x => x.ProcessedByUserId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<SaleReturnItem>(entity =>
        {
            entity.Property(x => x.Quantity).HasPrecision(18, 3);
            entity.Property(x => x.RefundAmount).HasPrecision(18, 2);
            entity.HasOne(x => x.SaleReturn).WithMany(x => x.Items)
                .HasForeignKey(x => x.SaleReturnId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.SaleItem).WithMany()
                .HasForeignKey(x => x.SaleItemId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Supplier>(entity =>
        {
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasIndex(x => x.Name);
            entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(250);
            entity.Property(x => x.Phone).HasMaxLength(50);
        });

        builder.Entity<PurchaseOrder>(entity =>
        {
            entity.HasIndex(x => x.PurchaseOrderNumber).IsUnique();
            entity.Property(x => x.PurchaseOrderNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Subtotal).HasPrecision(18, 2);
            entity.Property(x => x.DiscountTotal).HasPrecision(18, 2);
            entity.Property(x => x.TaxTotal).HasPrecision(18, 2);
            entity.Property(x => x.GrandTotal).HasPrecision(18, 2);
            entity.HasOne(x => x.Supplier).WithMany(x => x.PurchaseOrders)
                .HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Store).WithMany()
                .HasForeignKey(x => x.StoreId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CreatedByUser).WithMany()
                .HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<PurchaseOrderItem>(entity =>
        {
            entity.HasIndex(x => new { x.PurchaseOrderId, x.ProductVariantId }).IsUnique();
            entity.Property(x => x.OrderedQuantity).HasPrecision(18, 3);
            entity.Property(x => x.ReceivedQuantity).HasPrecision(18, 3);
            entity.Property(x => x.UnitCost).HasPrecision(18, 2);
            entity.Property(x => x.DiscountAmount).HasPrecision(18, 2);
            entity.Property(x => x.TaxAmount).HasPrecision(18, 2);
            entity.Property(x => x.LineTotal).HasPrecision(18, 2);
            entity.HasOne(x => x.PurchaseOrder).WithMany(x => x.Items)
                .HasForeignKey(x => x.PurchaseOrderId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.ProductVariant).WithMany()
                .HasForeignKey(x => x.ProductVariantId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<GoodsReceipt>(entity =>
        {
            entity.HasIndex(x => x.ReceiptNumber).IsUnique();
            entity.Property(x => x.ReceiptNumber).HasMaxLength(50).IsRequired();
            entity.HasOne(x => x.PurchaseOrder).WithMany(x => x.GoodsReceipts)
                .HasForeignKey(x => x.PurchaseOrderId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Store).WithMany()
                .HasForeignKey(x => x.StoreId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ReceivedByUser).WithMany()
                .HasForeignKey(x => x.ReceivedByUserId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<GoodsReceiptItem>(entity =>
        {
            entity.Property(x => x.QuantityReceived).HasPrecision(18, 3);
            entity.Property(x => x.UnitCost).HasPrecision(18, 2);
            entity.HasOne(x => x.GoodsReceipt).WithMany(x => x.Items)
                .HasForeignKey(x => x.GoodsReceiptId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.PurchaseOrderItem).WithMany()
                .HasForeignKey(x => x.PurchaseOrderItemId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ProductVariant).WithMany()
                .HasForeignKey(x => x.ProductVariantId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<SupplierReturn>(entity =>
        {
            entity.HasIndex(x => x.ReturnNumber).IsUnique();
            entity.Property(x => x.ReturnNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.TotalCost).HasPrecision(18, 2);
            entity.HasOne(x => x.Supplier).WithMany()
                .HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Store).WithMany()
                .HasForeignKey(x => x.StoreId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ProcessedByUser).WithMany()
                .HasForeignKey(x => x.ProcessedByUserId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<SupplierReturnItem>(entity =>
        {
            entity.Property(x => x.Quantity).HasPrecision(18, 3);
            entity.Property(x => x.UnitCost).HasPrecision(18, 2);
            entity.Property(x => x.LineTotal).HasPrecision(18, 2);
            entity.HasOne(x => x.SupplierReturn).WithMany(x => x.Items)
                .HasForeignKey(x => x.SupplierReturnId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.ProductVariant).WithMany()
                .HasForeignKey(x => x.ProductVariantId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<StockTransfer>(entity =>
        {
            entity.HasIndex(x => x.TransferNumber).IsUnique();
            entity.Property(x => x.TransferNumber).HasMaxLength(50).IsRequired();
            entity.HasOne(x => x.SourceStore).WithMany()
                .HasForeignKey(x => x.SourceStoreId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.DestinationStore).WithMany()
                .HasForeignKey(x => x.DestinationStoreId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CreatedByUser).WithMany()
                .HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.DispatchedByUser).WithMany()
                .HasForeignKey(x => x.DispatchedByUserId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.ReceivedByUser).WithMany()
                .HasForeignKey(x => x.ReceivedByUserId).OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<StockTransferItem>(entity =>
        {
            entity.HasIndex(x => new { x.StockTransferId, x.ProductVariantId }).IsUnique();
            entity.Property(x => x.RequestedQuantity).HasPrecision(18, 3);
            entity.Property(x => x.DispatchedQuantity).HasPrecision(18, 3);
            entity.Property(x => x.ReceivedQuantity).HasPrecision(18, 3);
            entity.Property(x => x.DamagedQuantity).HasPrecision(18, 3);
            entity.HasOne(x => x.StockTransfer).WithMany(x => x.Items)
                .HasForeignKey(x => x.StockTransferId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.ProductVariant).WithMany()
                .HasForeignKey(x => x.ProductVariantId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<StockTransferReceipt>(entity =>
        {
            entity.HasIndex(x => x.ReceiptNumber).IsUnique();
            entity.Property(x => x.ReceiptNumber).HasMaxLength(50).IsRequired();
            entity.HasOne(x => x.StockTransfer).WithMany(x => x.Receipts)
                .HasForeignKey(x => x.StockTransferId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ReceivedByUser).WithMany()
                .HasForeignKey(x => x.ReceivedByUserId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<StockTransferReceiptItem>(entity =>
        {
            entity.Property(x => x.QuantityReceived).HasPrecision(18, 3);
            entity.Property(x => x.QuantityDamaged).HasPrecision(18, 3);
            entity.HasOne(x => x.StockTransferReceipt).WithMany(x => x.Items)
                .HasForeignKey(x => x.StockTransferReceiptId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.StockTransferItem).WithMany()
                .HasForeignKey(x => x.StockTransferItemId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ProductVariant).WithMany()
                .HasForeignKey(x => x.ProductVariantId).OnDelete(DeleteBehavior.Restrict);
        });
    
        builder.Entity<CustomerTier>(entity =>
        {
            entity.HasIndex(x => x.Name).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.MinimumLifetimeSpend).HasPrecision(18, 2);
            entity.Property(x => x.PointsMultiplier).HasPrecision(18, 2);
            entity.Property(x => x.DefaultDiscountPercent).HasPrecision(5, 2);
        });

        builder.Entity<Customer>(entity =>
        {
            entity.HasIndex(x => x.CustomerNumber).IsUnique();
            entity.HasIndex(x => x.Email);
            entity.HasIndex(x => x.Phone);
            entity.Property(x => x.CustomerNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(250);
            entity.Property(x => x.Phone).HasMaxLength(50);
            entity.Property(x => x.LoyaltyPointsBalance).HasPrecision(18, 2);
            entity.Property(x => x.StoreCreditBalance).HasPrecision(18, 2);
            entity.Property(x => x.LifetimeSpend).HasPrecision(18, 2);
            entity.HasOne(x => x.CustomerTier).WithMany(x => x.Customers)
                .HasForeignKey(x => x.CustomerTierId).OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<LoyaltyTransaction>(entity =>
        {
            entity.Property(x => x.PointsChange).HasPrecision(18, 2);
            entity.Property(x => x.BalanceAfter).HasPrecision(18, 2);
            entity.HasOne(x => x.Customer).WithMany(x => x.LoyaltyTransactions)
                .HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Sale).WithMany()
                .HasForeignKey(x => x.SaleId).OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<CustomerVoucher>(entity =>
        {
            entity.HasIndex(x => x.Code).IsUnique();
            entity.Property(x => x.Code).HasMaxLength(80).IsRequired();
            entity.Property(x => x.DiscountAmount).HasPrecision(18, 2);
            entity.Property(x => x.DiscountPercent).HasPrecision(5, 2);
            entity.Property(x => x.MinimumSpend).HasPrecision(18, 2);
            entity.HasOne(x => x.Customer).WithMany(x => x.Vouchers)
                .HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.RedeemedSale).WithMany()
                .HasForeignKey(x => x.RedeemedSaleId).OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<CustomerFavoriteProduct>(entity =>
        {
            entity.HasIndex(x => new { x.CustomerId, x.ProductId }).IsUnique();
            entity.HasOne(x => x.Customer).WithMany(x => x.FavoriteProducts)
                .HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Product).WithMany()
                .HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Sale>(entity =>
        {
            entity.HasOne(x => x.Customer).WithMany()
                .HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<EmployeeProfile>(entity =>
        {
            entity.HasIndex(x => x.EmployeeNumber).IsUnique();
            entity.HasIndex(x => x.UserId).IsUnique();
            entity.Property(x => x.EmployeeNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.JobTitle).HasMaxLength(150).IsRequired();
            entity.HasOne(x => x.User).WithMany()
                .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.PrimaryStore).WithMany()
                .HasForeignKey(x => x.PrimaryStoreId).OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<EmployeeStoreAssignment>(entity =>
        {
            entity.HasIndex(x => new { x.EmployeeProfileId, x.StoreId, x.UnassignedAtUtc });
            entity.HasOne(x => x.EmployeeProfile).WithMany(x => x.StoreAssignments)
                .HasForeignKey(x => x.EmployeeProfileId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Store).WithMany()
                .HasForeignKey(x => x.StoreId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<EmployeeAttendance>(entity =>
        {
            entity.HasIndex(x => new { x.EmployeeProfileId, x.WorkDate }).IsUnique();
            entity.HasOne(x => x.EmployeeProfile).WithMany(x => x.AttendanceRecords)
                .HasForeignKey(x => x.EmployeeProfileId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<AuditLog>(entity =>
        {
            entity.HasIndex(x => x.CreatedAtUtc);
            entity.HasIndex(x => new { x.EntityName, x.EntityId });
            entity.Property(x => x.Action).HasMaxLength(100).IsRequired();
            entity.Property(x => x.EntityName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.EntityId).HasMaxLength(100);
            entity.Property(x => x.IpAddress).HasMaxLength(100);
            entity.Property(x => x.UserAgent).HasMaxLength(1000);
            entity.HasOne(x => x.User).WithMany()
                .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<GeneralLedgerAccount>(entity =>
        {
            entity.HasIndex(x => x.AccountCode).IsUnique();
            entity.Property(x => x.AccountCode).HasMaxLength(30).IsRequired();
            entity.Property(x => x.AccountName).HasMaxLength(200).IsRequired();
            entity.HasOne(x => x.ParentAccount).WithMany(x => x.ChildAccounts)
                .HasForeignKey(x => x.ParentAccountId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<JournalEntry>(entity =>
        {
            entity.HasIndex(x => x.EntryNumber).IsUnique();
            entity.Property(x => x.EntryNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(500).IsRequired();
            entity.HasOne(x => x.Store).WithMany()
                .HasForeignKey(x => x.StoreId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.CreatedByUser).WithMany()
                .HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PostedByUser).WithMany()
                .HasForeignKey(x => x.PostedByUserId).OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<JournalEntryLine>(entity =>
        {
            entity.Property(x => x.DebitAmount).HasPrecision(18, 2);
            entity.Property(x => x.CreditAmount).HasPrecision(18, 2);
            entity.HasOne(x => x.JournalEntry).WithMany(x => x.Lines)
                .HasForeignKey(x => x.JournalEntryId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.GeneralLedgerAccount).WithMany()
                .HasForeignKey(x => x.GeneralLedgerAccountId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<AccountsPayable>(entity =>
        {
            entity.HasIndex(x => x.PayableNumber).IsUnique();
            entity.Property(x => x.PayableNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.OriginalAmount).HasPrecision(18, 2);
            entity.Property(x => x.PaidAmount).HasPrecision(18, 2);
            entity.Property(x => x.BalanceAmount).HasPrecision(18, 2);
            entity.HasOne(x => x.Supplier).WithMany()
                .HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PurchaseOrder).WithMany()
                .HasForeignKey(x => x.PurchaseOrderId).OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<AccountsReceivable>(entity =>
        {
            entity.HasIndex(x => x.ReceivableNumber).IsUnique();
            entity.Property(x => x.ReceivableNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.OriginalAmount).HasPrecision(18, 2);
            entity.Property(x => x.CollectedAmount).HasPrecision(18, 2);
            entity.Property(x => x.BalanceAmount).HasPrecision(18, 2);
            entity.HasOne(x => x.Customer).WithMany()
                .HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.Sale).WithMany()
                .HasForeignKey(x => x.SaleId).OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<ExpenseRecord>(entity =>
        {
            entity.HasIndex(x => x.ExpenseNumber).IsUnique();
            entity.Property(x => x.ExpenseNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.TaxAmount).HasPrecision(18, 2);
            entity.Property(x => x.Description).HasMaxLength(500).IsRequired();
            entity.HasOne(x => x.Store).WithMany()
                .HasForeignKey(x => x.StoreId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ExpenseAccount).WithMany()
                .HasForeignKey(x => x.ExpenseAccountId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CreatedByUser).WithMany()
                .HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<NotificationMessage>(entity =>
        {
            entity.HasIndex(x => new { x.Status, x.ScheduledAtUtc });
            entity.Property(x => x.Recipient).HasMaxLength(500).IsRequired();
            entity.Property(x => x.Subject).HasMaxLength(500).IsRequired();
            entity.Property(x => x.TemplateCode).HasMaxLength(100);
            entity.Property(x => x.ReferenceType).HasMaxLength(100);
            entity.Property(x => x.ReferenceId).HasMaxLength(100);
        });

        builder.Entity<IntegrationSetting>(entity =>
        {
            entity.HasIndex(x => new { x.ProviderCode, x.SettingKey }).IsUnique();
            entity.Property(x => x.ProviderCode).HasMaxLength(100).IsRequired();
            entity.Property(x => x.SettingKey).HasMaxLength(150).IsRequired();
        });

        builder.Entity<ReceiptPrintJob>(entity =>
        {
            entity.HasIndex(x => new { x.Status, x.CreatedAtUtc });
            entity.Property(x => x.PrinterName).HasMaxLength(250).IsRequired();
            entity.Property(x => x.PrinterProtocol).HasMaxLength(50).IsRequired();
            entity.HasOne(x => x.Sale).WithMany()
                .HasForeignKey(x => x.SaleId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Store).WithMany()
                .HasForeignKey(x => x.StoreId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<BarcodeAlias>(entity =>
        {
            entity.HasIndex(x => x.Barcode).IsUnique();
            entity.HasIndex(x => new { x.ProductVariantId, x.IsPrimary });
            entity.Property(x => x.Barcode).HasMaxLength(100).IsRequired();
            entity.Property(x => x.BarcodeType).HasMaxLength(30).IsRequired();
            entity.HasOne(x => x.ProductVariant).WithMany()
                .HasForeignKey(x => x.ProductVariantId).OnDelete(DeleteBehavior.Cascade);
        });
}
}
