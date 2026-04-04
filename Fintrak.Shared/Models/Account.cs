using System.Security.Principal;
using Fintrak.Shared.Models;

namespace Fintrak.Models;

public class Account
{
    public int Id{get;set;}
    public string UserId{get;set;} = string.Empty;
    public string Name{get;set;} = string.Empty;
    public WindowsAccountType Type{get;set;}

    public decimal Balance {get;set;}

    public string CurrencyCode{get;set;} = string.Empty;

    public ApplicationUser? User {get;set;} 
    public ICollection<Transaction> Transactions{get;set;} = new List<Transaction>();

    public enum AccountType
    {
        Checking,
        Savings,
        Cash,
        Investment,
        CreditCard
    }

}