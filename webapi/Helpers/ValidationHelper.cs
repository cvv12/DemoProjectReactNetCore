using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace webapi.Helpers
{
    public class ValidationHelper
    {
        private static string ConnectionString;

        public static void Initialize(IConfiguration configuration)
        {
            if (string.IsNullOrEmpty(ConnectionString))
            {
                ConnectionString = configuration.GetConnectionString("DefaultConnection");
            }
        }
        public static bool ValidateNRIC(string nric)
        {
            // Remove whitespace and convert to uppercase
            nric = nric.Replace(" ", "").ToUpper();

            // Check if the NRIC is of the correct length and format
            if (!Regex.IsMatch(nric, @"^[STFG]\d{7}[A-Z]$"))
            {
                return false;
            }

            // Define the weight factors for each digit in the NRIC
            int[] weights = { 2, 7, 6, 5, 4, 3, 2 };

            // Extract the first character (which represents the type)
            char nricType = nric[0];

            // Define the corresponding factors for each NRIC type
            int typeFactor = (nricType == 'T' || nricType == 'G') ? 4 : 0;

            // Calculate the weighted sum
            int weightedSum = typeFactor;
            for (int i = 1; i <= 7; i++)
            {
                int digit = int.Parse(nric.Substring(i, 1));
                weightedSum += digit * weights[i - 1];
            }

            // Calculate the remainder after dividing the weighted sum by 11
            int remainder = weightedSum % 11;

            // Define the checksum characters for each NRIC type
            char[] checksumChars = { 'J', 'Z', 'I', 'H', 'G', 'F', 'E', 'D', 'C', 'B', 'A' };

            // Compare the calculated checksum character with the one in the NRIC
            char actualChecksum = nric[8];

            return actualChecksum == checksumChars[remainder];
        }
        public static bool CheckNRICExists(string nric)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("SELECT COUNT(*) FROM Customer WHERE NRIC = @NRIC", connection))
                {
                    command.Parameters.AddWithValue("@NRIC", nric);
                    int count = (int)command.ExecuteScalar();
                    return count > 0;
                }
            }
        }
    }
}
