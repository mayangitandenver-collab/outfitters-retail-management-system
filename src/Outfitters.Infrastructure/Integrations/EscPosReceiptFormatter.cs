using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Outfitters.Infrastructure.Persistence;

namespace Outfitters.Infrastructure.Integrations;

public interface IReceiptFormatter
{
    Task<string> FormatSaleAsync(
        Guid saleId,
        CancellationToken cancellationToken = default);
}

public sealed class EscPosReceiptFormatter : IReceiptFormatter
{
    private readonly ApplicationDbContext _db;

    public EscPosReceiptFormatter(ApplicationDbContext db) => _db = db;

    public async Task<string> FormatSaleAsync(
        Guid saleId,
        CancellationToken cancellationToken = default)
    {
        var sale = await _db.Sales
            .AsNoTracking()
            .Include(x => x.Store)
            .Include(x => x.Items)
                .ThenInclude(x => x.ProductVariant)
                .ThenInclude(x => x.Product)
            .SingleOrDefaultAsync(x => x.Id == saleId, cancellationToken);

        if (sale is null)
        {
            throw new InvalidOperationException("Sale was not found.");
        }

        var builder = new StringBuilder();
        builder.AppendLine("\u001b@");
        builder.AppendLine("\u001ba\u0001");
        builder.AppendLine(sale.Store.Name);
        builder.AppendLine("OUTFITTERS");
        builder.AppendLine("--------------------------------");
        builder.AppendLine($"\u001ba\u0000Receipt: {sale.ReceiptNumber}");
        builder.AppendLine($"Date: {sale.CreatedAtUtc:yyyy-MM-dd HH:mm}");
        builder.AppendLine("--------------------------------");

        foreach (var item in sale.Items)
        {
            builder.AppendLine(item.ProductVariant.Product.Name);
            builder.AppendLine(
                $"{item.Quantity:0.###} x {item.UnitPrice.ToString("N2", CultureInfo.InvariantCulture)}  {item.LineTotal.ToString("N2", CultureInfo.InvariantCulture)}");
        }

        builder.AppendLine("--------------------------------");
        builder.AppendLine($"TOTAL: {sale.GrandTotal.ToString("N2", CultureInfo.InvariantCulture)}");
        builder.AppendLine();
        builder.AppendLine("\u001ba\u0001Thank you!");
        builder.AppendLine("\u001dV\u0000");

        return builder.ToString();
    }
}
