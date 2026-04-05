

using Microsoft.EntityFrameworkCore;
using Fintrak.Api.Data;
using Fintrak.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;

namespace Fintrak.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BudgetGoalsController : ControllerBase
{
    private readonly AppDbContext _db;

    public BudgetGoalsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var goals = await _db.BudgetGoals
        .Include(b => b.Category)
        .ToListAsync();

        return Ok(goals);
    }

    [HttpGet("progress")]
    public async Task<IActionResult> GetProgress(int month, int year)
    {
        var goals = await _db.BudgetGoals
        .Include(b => b.Category)
        .Where(b => b.Month == month && b.Year == year)
        .ToListAsync();

        var categoryIds = goals.Select(g => g.CategoryId).ToList();

        var spending = await _db.Transactions
        .Where(t =>
            t.Date.Month == month &&
            t.Date.Year == year &&
            t.Type == TransactionType.Expense &&
            categoryIds.Contains(t.CategoryId))
            .GroupBy(t => t.CategoryId)
            .Select(g => new
            {
                CategoryId = g.Key,
                TotalSpent = g.Sum(t => t.Amount)

            })
            .ToListAsync();
        var progress = goals.Select(goal => new
        {
            goal.Id,
            goal.CategoryId,
            CategoryName = goal.Category!.Name,
            CategoryColor = goal.Category!.Color,
            goal.MonthlyLimit,
            TotalSpent = spending
            .FirstOrDefault(s => s.CategoryId == goal.CategoryId)?.TotalSpent ?? 0,
            PercentageUsed = Math.Round(
                spending.FirstOrDefault(s => s.CategoryId == goal.CategoryId)?.TotalSpent ?? 0
                / goal.MonthlyLimit * 100, 1
            )
        });
        return Ok(progress);

    }

    [HttpPost]
    public async Task<IActionResult> Create(BudgetGoal goal)
    {
        var exists = await _db.BudgetGoals.AnyAsync(b =>
        b.CategoryId == goal.CategoryId &&
        b.Month == goal.Month &&
        b.Year == goal.Year &&
        b.UserId == goal.UserId
        );

        if (exists)
        {
            return Conflict("A budget goal for this category and month exists!");
        }

        _db.BudgetGoals.Add(goal);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new { id = goal.Id }, goal);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, BudgetGoal updated)
    {
        var goal = await _db.BudgetGoals.FindAsync(id);

        if (goal is null)
        {
            return NotFound();
        }

        goal.MonthlyLimit = updated.MonthlyLimit;

        await _db.SaveChangesAsync();

        return NoContent();

    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var goal = await _db.BudgetGoals.FindAsync(id);

        if(goal is null)
        {
            return NotFound();
        }

        _db.BudgetGoals.Remove(goal);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}