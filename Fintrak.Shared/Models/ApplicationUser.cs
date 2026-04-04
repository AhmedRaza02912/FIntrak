using System.Security.Principal;
using Fintrak.Models;
namespace Fintrak.Shared.Models;

public class ApplicationUser
{
    public string Id{get;set;} = Guid.NewGuid().ToString();
    public string FullName{get;set;} = string.Empty;
    public string Email{get;set;} = string.Empty;
    public string PasswordHash{get;set;} = string.Empty;
    public DateTime CreatedAt{get;set;} = DateTime.UtcNow;

    public ICollection<Account> Accounts{get;set;} = new List<Account>();


}