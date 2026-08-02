using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Outfitters.Application.Accounting;
using Outfitters.Domain.Entities;
using Outfitters.Domain.Enums;
using Outfitters.Infrastructure.Persistence;

namespace Outfitters.API.Controllers;

[ApiController]
[Authorize(Roles = "SuperAdministrator,Administrator,Accountant,Auditor")]
[Route("api/accounting")]
public sealed class AccountingController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public AccountingController(ApplicationDbContext db) => _db = db;

    [HttpGet("accounts")]
    public async Task<IActionResult> GetAccounts()
    {
        var result = await _db.GeneralLedgerAccounts
            .AsNoTracking()
            .OrderBy(x => x.AccountCode)
            .Select(x => new
            {
                x.Id,
                x.AccountCode,
                x.AccountName,
                x.AccountType,
                x.ParentAccountId,
                x.IsPostingAccount,
                x.IsActive
            })
            .ToListAsync();

        return Ok(result);
    }

    [HttpPost("accounts")]
    public async Task<IActionResult> CreateAccount(
        CreateLedgerAccountRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.AccountCode) ||
            string.IsNullOrWhiteSpace(request.AccountName))
        {
            return BadRequest("Account code and name are required.");
        }

        if (await _db.GeneralLedgerAccounts.AnyAsync(x =>
            x.AccountCode == request.AccountCode.Trim()))
        {
            return Conflict("Account code already exists.");
        }

        var account = new GeneralLedgerAccount
        {
            AccountCode = request.AccountCode.Trim(),
            AccountName = request.AccountName.Trim(),
            AccountType = request.AccountType,
            ParentAccountId = request.ParentAccountId,
            IsPostingAccount = request.IsPostingAccount
        };

        _db.GeneralLedgerAccounts.Add(account);
        await _db.SaveChangesAsync();

        return Ok(account);
    }

    [HttpPost("journal-entries")]
    public async Task<IActionResult> CreateJournalEntry(
        CreateJournalEntryRequest request)
    {
        if (request.Lines.Count < 2)
        {
            return BadRequest("At least two journal lines are required.");
        }

        var debitTotal = request.Lines.Sum(x => x.DebitAmount);
        var creditTotal = request.Lines.Sum(x => x.CreditAmount);

        if (debitTotal <= 0 || debitTotal != creditTotal)
        {
            return BadRequest("Journal entry debits and credits must balance.");
        }

        foreach (var line in request.Lines)
        {
            if (line.DebitAmount < 0 ||
                line.CreditAmount < 0 ||
                (line.DebitAmount > 0 && line.CreditAmount > 0))
            {
                return BadRequest("Each line must contain either a debit or credit.");
            }

            if (!await _db.GeneralLedgerAccounts.AnyAsync(x =>
                x.Id == line.GeneralLedgerAccountId &&
                x.IsActive &&
                x.IsPostingAccount))
            {
                return BadRequest("A posting account was not found.");
            }
        }

        var entry = new JournalEntry
        {
            EntryNumber = await GenerateEntryNumber(),
            EntryDateUtc = request.EntryDateUtc,
            Description = request.Description.Trim(),
            ReferenceNumber = request.ReferenceNumber?.Trim(),
            StoreId = request.StoreId,
            CreatedByUserId = GetUserId()
        };

        foreach (var line in request.Lines)
        {
            entry.Lines.Add(new JournalEntryLine
            {
                GeneralLedgerAccountId = line.GeneralLedgerAccountId,
                Description = line.Description?.Trim(),
                DebitAmount = line.DebitAmount,
                CreditAmount = line.CreditAmount
            });
        }

        _db.JournalEntries.Add(entry);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            entry.Id,
            entry.EntryNumber,
            DebitTotal = debitTotal,
            CreditTotal = creditTotal,
            entry.Status
        });
    }

    [HttpPost("journal-entries/{id:guid}/post")]
    public async Task<IActionResult> PostJournalEntry(Guid id)
    {
        var entry = await _db.JournalEntries
            .Include(x => x.Lines)
            .SingleOrDefaultAsync(x => x.Id == id);

        if (entry is null)
        {
            return NotFound();
        }

        if (entry.Status != JournalEntryStatus.Draft)
        {
            return BadRequest("Only draft journal entries can be posted.");
        }

        if (entry.Lines.Sum(x => x.DebitAmount) !=
            entry.Lines.Sum(x => x.CreditAmount))
        {
            return BadRequest("Journal entry is not balanced.");
        }

        entry.Status = JournalEntryStatus.Posted;
        entry.PostedByUserId = GetUserId();
        entry.PostedAtUtc = DateTime.UtcNow;
        entry.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("expenses")]
    public async Task<IActionResult> CreateExpense(CreateExpenseRequest request)
    {
        if (request.Amount <= 0 || request.TaxAmount < 0)
        {
            return BadRequest("Expense amount is invalid.");
        }

        var expense = new ExpenseRecord
        {
            ExpenseNumber = await GenerateExpenseNumber(),
            StoreId = request.StoreId,
            ExpenseAccountId = request.ExpenseAccountId,
            ExpenseDateUtc = request.ExpenseDateUtc,
            Amount = request.Amount,
            TaxAmount = request.TaxAmount,
            Description = request.Description.Trim(),
            ReferenceNumber = request.ReferenceNumber?.Trim(),
            CreatedByUserId = GetUserId()
        };

        _db.ExpenseRecords.Add(expense);
        await _db.SaveChangesAsync();

        return Ok(expense);
    }

    [HttpGet("trial-balance")]
    public async Task<IActionResult> TrialBalance(
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc)
    {
        var query = _db.JournalEntryLines
            .AsNoTracking()
            .Where(x => x.JournalEntry.Status == JournalEntryStatus.Posted);

        if (fromUtc.HasValue)
        {
            query = query.Where(x =>
                x.JournalEntry.EntryDateUtc >= fromUtc.Value);
        }

        if (toUtc.HasValue)
        {
            query = query.Where(x =>
                x.JournalEntry.EntryDateUtc <= toUtc.Value);
        }

        var result = await query
            .GroupBy(x => new
            {
                x.GeneralLedgerAccountId,
                x.GeneralLedgerAccount.AccountCode,
                x.GeneralLedgerAccount.AccountName,
                x.GeneralLedgerAccount.AccountType
            })
            .Select(group => new
            {
                group.Key.GeneralLedgerAccountId,
                group.Key.AccountCode,
                group.Key.AccountName,
                group.Key.AccountType,
                Debit = group.Sum(x => x.DebitAmount),
                Credit = group.Sum(x => x.CreditAmount),
                Balance = group.Sum(x => x.DebitAmount - x.CreditAmount)
            })
            .OrderBy(x => x.AccountCode)
            .ToListAsync();

        return Ok(result);
    }

    [HttpGet("income-statement")]
    public async Task<IActionResult> IncomeStatement(
        [FromQuery] DateTime fromUtc,
        [FromQuery] DateTime toUtc)
    {
        var lines = _db.JournalEntryLines
            .AsNoTracking()
            .Where(x =>
                x.JournalEntry.Status == JournalEntryStatus.Posted &&
                x.JournalEntry.EntryDateUtc >= fromUtc &&
                x.JournalEntry.EntryDateUtc <= toUtc);

        var revenue = await lines
            .Where(x => x.GeneralLedgerAccount.AccountType == AccountType.Revenue)
            .SumAsync(x => (decimal?)(x.CreditAmount - x.DebitAmount)) ?? 0m;

        var expenses = await lines
            .Where(x => x.GeneralLedgerAccount.AccountType == AccountType.Expense)
            .SumAsync(x => (decimal?)(x.DebitAmount - x.CreditAmount)) ?? 0m;

        return Ok(new
        {
            FromUtc = fromUtc,
            ToUtc = toUtc,
            Revenue = revenue,
            Expenses = expenses,
            NetIncome = revenue - expenses
        });
    }

    [HttpGet("balance-sheet")]
    public async Task<IActionResult> BalanceSheet([FromQuery] DateTime asOfUtc)
    {
        var lines = _db.JournalEntryLines
            .AsNoTracking()
            .Where(x =>
                x.JournalEntry.Status == JournalEntryStatus.Posted &&
                x.JournalEntry.EntryDateUtc <= asOfUtc);

        var assets = await lines
            .Where(x => x.GeneralLedgerAccount.AccountType == AccountType.Asset)
            .SumAsync(x => (decimal?)(x.DebitAmount - x.CreditAmount)) ?? 0m;

        var liabilities = await lines
            .Where(x => x.GeneralLedgerAccount.AccountType == AccountType.Liability)
            .SumAsync(x => (decimal?)(x.CreditAmount - x.DebitAmount)) ?? 0m;

        var equity = await lines
            .Where(x => x.GeneralLedgerAccount.AccountType == AccountType.Equity)
            .SumAsync(x => (decimal?)(x.CreditAmount - x.DebitAmount)) ?? 0m;

        return Ok(new
        {
            AsOfUtc = asOfUtc,
            Assets = assets,
            Liabilities = liabilities,
            Equity = equity,
            LiabilitiesAndEquity = liabilities + equity,
            IsBalanced = assets == liabilities + equity
        });
    }

    private Guid GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id)
            ? id
            : throw new UnauthorizedAccessException(
                "User identifier is missing.");
    }

    private async Task<string> GenerateEntryNumber()
    {
        var date = DateTime.UtcNow.ToString(
            "yyyyMMdd",
            CultureInfo.InvariantCulture);
        var count = await _db.JournalEntries.CountAsync(x =>
            x.CreatedAtUtc.Date == DateTime.UtcNow.Date);
        return $"JE-{date}-{count + 1:00000}";
    }

    private async Task<string> GenerateExpenseNumber()
    {
        var date = DateTime.UtcNow.ToString(
            "yyyyMMdd",
            CultureInfo.InvariantCulture);
        var count = await _db.ExpenseRecords.CountAsync(x =>
            x.CreatedAtUtc.Date == DateTime.UtcNow.Date);
        return $"EXP-{date}-{count + 1:00000}";
    }
}
