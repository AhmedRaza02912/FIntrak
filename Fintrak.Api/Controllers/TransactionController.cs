using Fintrak.Api.Data;
using Fintrak.Shared.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace Fintrak.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly AppDbContext _db;

    public TransactionsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var transactions = await _db.Transactions
        .Include(t => t.Category)
        .Include(t => t.Account)
        .OrderByDescending(t => t.Date)
        .ToListAsync();

        return Ok(transactions);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var transaction = await _db.Transactions
        .Include(t => t.Category)
        .Include(t => t.Account)
        .FirstOrDefaultAsync(t => t.Id == id);

        if (transaction is null)
        {
            return NotFound();
        }
        return Ok(transaction);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var now = DateTime.UtcNow;

        var transactions = await _db.Transactions
        .Where(t => t.Date.Month == now.Month && t.Date.Year == now.Year)
        .ToListAsync();
        var summary = new
        {
            TotalIncome = transactions
                      .Where(t => t.Type == TransactionType.Income)
                      .Sum(t => t.Amount),

            TotalExpenses = transactions
                      .Where(t => t.Type == TransactionType.Expense)
                      .Sum(t => t.Amount),

            SavingsRate = transactions.Any()
                      ? Math.Round(
                          transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount) > 0
                              ? (1 - transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount)
                                  / transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount)) * 100
                              : 0, 1)
                      : 0
        };
        return Ok(summary);
    }

    [HttpGet("monthly")]
    public async Task<IActionResult> GetMonthlyBreakdown()
    {
        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
        var breakdown = await _db.Transactions
        .Where(t => t.Date >= sixMonthsAgo)
        .GroupBy(t => new {t.Date.Year, t.Date.Month, t.Type})
        .Select( g=> new
        {
            g.Key.Year,
            g.Key.Month,
            g.Key.Type,
            Total = g.Sum( t=> t.Amount)
        })
        .OrderBy( x => x.Year)
        .ThenBy(x => x.Month)
        .ToListAsync();

        return Ok(breakdown);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Transaction transaction)
    {
        transaction.CreatedAt = DateTime.UtcNow;

        var account = await _db.Accounts.FindAsync(transaction.AccountId);

        if(account is null)
        {
            return BadRequest("Account not found.");
        }
        if(transaction.Type == TransactionType.Income)
        account.Balance += transaction.Amount;
        else
        account.Balance -= transaction.Amount;

        _db.Transactions.Add(transaction);

        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new {id = transaction.Id}, transaction);
    }

    [HttpPut("{id}")]
    public async Task <IActionResult> Update(int id, Transaction updated)
{
    var transaction = await _db.Transactions.FindAsync(id);

    if(transaction is null)
    {
        return NotFound();
    }

    transaction.Amount = updated.Amount;
    transaction.Description = updated.Description;
    transaction.Date = updated.Date;
    transaction.CategoryId = updated.CategoryId;
    transaction.Type = updated.Type;

    await _db.SaveChangesAsync();

    return NoContent();
}

[HttpDelete("{id}")]
public async Task<IActionResult> Delete(int id)
    {
        var transaction = await _db.Transactions.FindAsync(id);

        if(transaction is null)
        {
            return NotFound();
        }

        var account = await _db.Accounts.FindAsync(transaction.AccountId);
        if(account is not null)
        {
            if(transaction.Type == TransactionType.Income)
            {
                account.Balance -= transaction.Amount;
            }
            else
            {
                account.Balance  += transaction.Amount;
            }

            _db.Transactions.Remove(transaction);
            await _db.SaveChangesAsync();

            
        }
        return NoContent();
    }
    

}
