using Microsoft.EntityFrameworkCore;
using Fintrak.Shared.Models;
using Fintrak.Api.Data;
using Fintrak.Models;

namespace Fintrak.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ApplicationUser> Users { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<BudgetGoal> BudgetGoals { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ApplicationUser>()
        .HasKey(u => u.Id);

        // Account -> User ( many for one user)
        modelBuilder.Entity<Account>()
        .HasOne(a => a.User)
        .WithMany(u => u.Accounts)
        .HasForeignKey(a => a.UserId)
        .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Transaction>()
        .HasOne(t => t.Account)
        .WithMany(a => a.Transactions)
        .HasForeignKey(t => t.AccountId)
        .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BudgetGoal>()
        .HasOne(b => b.Category)
        .WithMany(c => c.BudgetGoals)
        .HasForeignKey(b => b.CategoryId)
        .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Account>()
        .Property(a => a.Balance)
        .HasPrecision(18, 2);

        modelBuilder.Entity<Transaction>()
        .Property(t => t.Amount)
        .HasPrecision(18, 2);

        modelBuilder.Entity<BudgetGoal>()
        .Property(b => b.MonthlyLimit)
        .HasPrecision(18, 2);
    }

}