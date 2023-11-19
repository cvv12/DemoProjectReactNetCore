using System.Data.SqlClient;

namespace webapi.Helpers
{
    public class GeneratorHelper
    {
        private static string Passphrase;
        private static string ConnectionString;
        private static string BankCode;
        public static void Initialize(IConfiguration configuration)
        {
            if (string.IsNullOrEmpty(Passphrase))
            {
                Passphrase = configuration.GetValue<string>("Encryption:Passphrase");
            }
            if (string.IsNullOrEmpty(ConnectionString))
            {
                ConnectionString = configuration.GetConnectionString("DefaultConnection");
            }
            if (string.IsNullOrEmpty(BankCode))
            {
                BankCode = configuration.GetValue<string>("BankMaster:BankCode");
            }
        }

        public static string GenerateAccountNumber(string accountType)
        {
            string accountTypeCode = GetAccountTypeCode(accountType.ToUpper());
            string accountNumber;

            do
            {
                string sevenDigitNumber = GenerateRandomNumber(7);
                string checksum = CalculateChecksum("12" + accountTypeCode + sevenDigitNumber);
                accountNumber = BankCode + accountTypeCode + sevenDigitNumber + checksum;
            }
            while (CheckAccountNumberExists(accountNumber));

            return accountNumber;
        }

        private static string GetAccountTypeCode(string accountType)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("SELECT Code FROM AccountTypeCodes WHERE AccountType = @AccountType", connection))
                {
                    command.Parameters.AddWithValue("@AccountType", accountType);
                    var result = command.ExecuteScalar();
                    if (result != null)
                    {
                        return result.ToString();
                    }
                    else
                    {
                        throw new ArgumentException("Invalid account type");
                    }
                }
            }
        }

        private static bool CheckAccountNumberExists(string accountNumber)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("SELECT COUNT(*) FROM Account WHERE AccountNumber = @AccountNumber", connection))
                {
                    command.Parameters.AddWithValue("@AccountNumber", accountNumber);
                    int count = (int)command.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        private static string GenerateRandomNumber(int length)
        {
            Random random = new Random();
            return new string(Enumerable.Repeat("0123456789", length)
                                          .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private static string CalculateChecksum(string input)
        {
            int sum = input.Sum(c => c - '0'); 
            int checksum = sum % 10; 
            return checksum.ToString();
        }
    }
}
