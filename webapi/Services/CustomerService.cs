using System.Data.SqlClient;
using System.Data;
using webapi.Interfaces;
using webapi.Models;

namespace webapi.Services
{
    public class CustomerService: ICustomerService
    {
        private readonly string _connectionString;

        public CustomerService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public async Task<int> AddCustomerAsync(Customer customer)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand("AddCustomer", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@CustomerName", customer.CustomerName.ToUpper().Trim());
                    command.Parameters.AddWithValue("@NRIC", customer.NRIC);

                    await connection.OpenAsync();
                    var result = await command.ExecuteScalarAsync();

                    if (result != null && int.TryParse(result.ToString(), out int customerId))
                    {
                        return customerId;
                    }
                    else
                    {
                        throw new Exception("Failed to retrieve a valid customer ID from the database.");
                    }
                }
            }
        }

        public async Task<IEnumerable<Customer>> GetCustomersAsync(int pageNumber, int pageSize, string searchTerm)
        {
            var customers = new List<Customer>();

            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand("GetCustomers", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@PageNumber", pageNumber);
                    command.Parameters.AddWithValue("@PageSize", pageSize);
                    command.Parameters.AddWithValue("@SearchTerm", string.IsNullOrEmpty(searchTerm) ? (object)DBNull.Value : searchTerm);

                    await connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            customers.Add(new Customer
                            {
                                CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                                CustomerName = reader.GetString(reader.GetOrdinal("CustomerName")),
                                NRIC = reader.GetString(reader.GetOrdinal("NRIC"))
                            });
                        }
                    }
                }
            }

            return customers;
        }


    }
}
