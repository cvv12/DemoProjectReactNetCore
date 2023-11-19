namespace webapi.Models
{
    public class Account
    {
        public int? AccountId { get; set; }
        public string? AccountNumber { get; set; }
        public string AccountType { get; set; }
        public string? EncryptedBalance { get; set; }
        public decimal Balance { get; set; }
        public int? CustomerId { get; set; }
    }
}
