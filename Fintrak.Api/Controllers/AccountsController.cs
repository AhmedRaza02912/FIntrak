using Fintrak.Api.Data;
using Fintrak.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace Fintrak.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly AppDbContext _db;
    public AccountsController(AppDbContext db)
    {
        _db = db;
    }

    // Get
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var accounts = await _db.Accounts.Include(a => a.Transactions)
        .ToListAsync();

        return Ok(accounts);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var account = await _db.Accounts.Include(a =>a.Transactions)
        .FirstOrDefaultAsync(a => a.Id == id);

        if(account is null)
        {
            return NotFound(new {Message = "Success!"});
        }
        else
        {
            return Ok(account);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create(Account account)
    {
        _db.Accounts.Add(account);
        await _db.SaveChangesAsync();
        // Returns 201 created at /api/account/{id}
        return CreatedAtAction(nameof(GetById), new {id = account.Id}, account);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Account updated)
    {
        var account = await _db.Accounts.FindAsync(id);
        if(account is null)
        {
            return NotFound();
        }

        account.Name = updated.Name;
        account.Type = updated.Type;
        account.Balance = updated.Balance;
        account.CurrencyCode = updated.CurrencyCode;

        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("id")]
    public async Task<IActionResult> Delete(int id)
    {
        var account = await _db.Accounts.FindAsync(id);
        if(account is null)
        {
            return NotFound();
        }

        _db.Accounts.Remove(account);
        await _db.SaveChangesAsync();

        return NoContent();
    }

}