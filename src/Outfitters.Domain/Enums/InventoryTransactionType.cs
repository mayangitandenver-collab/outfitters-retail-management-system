namespace Outfitters.Domain.Enums;

public enum InventoryTransactionType
{
    OpeningBalance = 1,
    Purchase = 2,
    Sale = 3,
    Return = 4,
    Adjustment = 5,
    TransferIn = 6,
    TransferOut = 7,
    Damage = 8,
    StockCount = 9
}
