namespace Fintrak.Shared.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public CategoryType Type { get; set; }

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<BudgetGoal> BudgetGoals{get;set;} = new List<BudgetGoal>();
}

public enum CategoryType
{
    Income,
    Expense
}