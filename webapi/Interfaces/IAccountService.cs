using webapi.Models;
using webapi.RequestModel;
using webapi.ViewModels;

namespace webapi.Interfaces
{
    public interface IAccountService
    {
        Task<int> AddAccountAsync(AccountRequest account);
        Task<bool> UpdateAccountBalanceAsync(int accountId, decimal newBalance);
        Task<(IEnumerable<AccountViewModel> accounts, int totalCount)> GetAccountsAsync(int pageNumber, int pageSize);
        Task<Account> GetAccountByIdAsync(int accountId);
        Task DeleteAccountAsync(int accountId);
        Task<bool> CustomerHasAccountTypeAsync(int customerId, string accountType);
    }
}
