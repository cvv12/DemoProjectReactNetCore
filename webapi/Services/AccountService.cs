using System.Data.SqlClient;
using System.Data;
using webapi.Models;
using webapi.Interfaces;
using webapi.ViewModels;
using webapi.RequestModel;
namespace webapi.Services
{
    public class AccountService: IAccountService
    {
        private readonly string _connectionString;

        public AccountService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public async Task<bool> UpdateAccountBalanceAsync(int accountId, decimal newBalance)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    using (var command = new SqlCommand("UpdateAccountBalance", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@AccountId", accountId);
                        command.Parameters.AddWithValue("@NewEncryptedBalance", EncryptionHelper.Encrypt(newBalance.ToString()));
                        await connection.OpenAsync();
                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating account balance: {ex.Message}");
                return false;
            }
        }

        public async Task<int> AddAccountAsync(AccountRequest model)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand("AddAccount", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@CustomerId", model.CustomerId);
                    command.Parameters.AddWithValue("@AccountType", model.Account.AccountType.ToUpper());
                    command.Parameters.AddWithValue("@AccountNumber", model.Account.AccountNumber);
                    var encryptedBalance = EncryptionHelper.Encrypt(model.Account.Balance.ToString());
                    command.Parameters.AddWithValue("@EncryptedBalance", string.IsNullOrEmpty(encryptedBalance) ? DBNull.Value : encryptedBalance);

                    await connection.OpenAsync();
                    var result = await command.ExecuteScalarAsync();

                    if (result != null && int.TryParse(result.ToString(), out int accountId))
                    {
                        return accountId;
                    }
                    else
                    {
                        throw new InvalidOperationException("Failed to retrieve a valid account ID from the database.");
                    }
                }
            }
        }

        public async Task<(IEnumerable<AccountViewModel> accounts, int totalCount)> GetAccountsAsync(int pageNumber, int pageSize)
        {
            var accounts = new List<AccountViewModel>();
            int totalCount = 0;

            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand("GetAccounts", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PageNumber", pageNumber);
                    command.Parameters.AddWithValue("@PageSize", pageSize);
                    await connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            if (totalCount == 0)
                            {
                                totalCount = reader.GetInt32(reader.GetOrdinal("TotalCount")); 
                            }

                            var encryptedBalance = reader.GetString(reader.GetOrdinal("EncryptedBalance"));
                            accounts.Add(new AccountViewModel
                            {
                                AccountId = reader.GetInt32(reader.GetOrdinal("AccountId")),
                                AccountNumber = reader.GetString(reader.GetOrdinal("AccountNumber")),
                                AccountType = reader.GetString(reader.GetOrdinal("AccountType")),
                                Balance = decimal.Parse(EncryptionHelper.Decrypt(encryptedBalance)),
                                CustomerName = reader.GetString(reader.GetOrdinal("CustomerName")),
                                NRIC = reader.GetString(reader.GetOrdinal("NRIC")),
                                TotalCount = totalCount 
                            });
                        }
                    }
                }
            }

            return (accounts, totalCount);
        }

        public async Task<Account> GetAccountByIdAsync(int accountId)
        {
            Account account = null;

            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand("GetAccountById", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@AccountId", accountId);

                    await connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            var encryptedBalance = reader.GetString(reader.GetOrdinal("EncryptedBalance"));
                            account = new Account
                            {
                                AccountId = reader.GetInt32(reader.GetOrdinal("AccountId")),
                                AccountNumber = reader.GetString(reader.GetOrdinal("AccountNumber")),
                                AccountType = reader.GetString(reader.GetOrdinal("AccountType")),
                                EncryptedBalance = encryptedBalance,
                                Balance = decimal.Parse(EncryptionHelper.Decrypt(encryptedBalance)) 
                            };
                        }
                    }
                }
            }

            return account;
        }

        public async Task DeleteAccountAsync(int accountId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand("DeleteAccount", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@AccountId", accountId);
                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                }
            }
        }


        public async Task<IEnumerable<Account>> SearchAccountsAsync(string searchTerm)
        {
            var accounts = new List<Account>();

            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand("SearchAccounts", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@SearchTerm", searchTerm);
                    await connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var encryptedBalance = reader.GetString(reader.GetOrdinal("EncryptedBalance"));
                            accounts.Add(new Account
                            {
                                AccountId = reader.GetInt32(reader.GetOrdinal("AccountId")),
                                AccountNumber = reader.GetString(reader.GetOrdinal("AccountNumber")),
                                AccountType = reader.GetString(reader.GetOrdinal("AccountType")),
                                EncryptedBalance = encryptedBalance,
                                Balance = decimal.Parse(EncryptionHelper.Decrypt(encryptedBalance))
                            });
                        }
                    }
                }
            }

            return accounts;
        }
        public async Task<bool> CustomerHasAccountTypeAsync(int customerId, string accountType)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand("[dbo].[CheckCustomerAccountTypeExists]", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@CustomerId", customerId);
                    command.Parameters.AddWithValue("@AccountType", accountType);

                    await connection.OpenAsync();
                    var result = (bool)await command.ExecuteScalarAsync();
                    return result;
                }
            }
        }

    }
}
