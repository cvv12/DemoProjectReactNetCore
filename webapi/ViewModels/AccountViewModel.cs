namespace webapi.ViewModels
{
    public class AccountViewModel
    {
        public int AccountId { get; set; }
        public string AccountNumber { get; set; }
        public string AccountType { get; set; }
        public decimal Balance { get; set; }
        public string CustomerName { get; set; }
        public string NRIC { get; set; }
        public int TotalCount { get; set; } 
    }


}
