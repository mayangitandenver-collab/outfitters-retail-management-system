using Outfitters.Domain.Common;
using Outfitters.Domain.Enums;

namespace Outfitters.Domain.Entities;

public sealed class GeneralLedgerAccount : BaseEntity
{
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public AccountType AccountType { get; set; }
    public Guid? ParentAccountId { get; set; }
    public GeneralLedgerAccount? ParentAccount { get; set; }
    public bool IsPostingAccount { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public ICollection<GeneralLedgerAccount> ChildAccounts { get; set; } =
        new List<GeneralLedgerAccount>();
}
