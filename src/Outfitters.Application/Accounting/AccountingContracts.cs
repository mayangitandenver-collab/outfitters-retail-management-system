using Outfitters.Domain.Enums;

namespace Outfitters.Application.Accounting;

public sealed record CreateLedgerAccountRequest(
    string AccountCode,
    string AccountName,
    AccountType AccountType,
    Guid? ParentAccountId,
    bool IsPostingAccount);

public sealed record JournalEntryLineRequest(
    Guid GeneralLedgerAccountId,
    string? Description,
    decimal DebitAmount,
    decimal CreditAmount);

public sealed record CreateJournalEntryRequest(
    DateTime EntryDateUtc,
    string Description,
    string? ReferenceNumber,
    Guid? StoreId,
    IReadOnlyCollection<JournalEntryLineRequest> Lines);

public sealed record CreateExpenseRequest(
    Guid StoreId,
    Guid ExpenseAccountId,
    DateTime ExpenseDateUtc,
    decimal Amount,
    decimal TaxAmount,
    string Description,
    string? ReferenceNumber);

public sealed record RecordPayablePaymentRequest(
    decimal Amount,
    string? ReferenceNumber);

public sealed record RecordReceivableCollectionRequest(
    decimal Amount,
    string? ReferenceNumber);
