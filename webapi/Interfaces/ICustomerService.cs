using webapi.Models;

namespace webapi.Interfaces
{
    public interface ICustomerService
    {
        Task<int> AddCustomerAsync(Customer customer);
        Task<IEnumerable<Customer>> GetCustomersAsync(int pageNumber, int pageSize, string searchTerm);
    }
}
