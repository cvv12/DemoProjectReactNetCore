using webapi.Models;

namespace webapi.RequestModel
{
    public class AccountRequest
    {
        public int CustomerId { get; set; }
        public Account Account { get; set; }
    }
}
