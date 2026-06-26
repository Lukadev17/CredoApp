namespace CredoApp.Models
{
    public class LoanApplication
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }

        public string LoanType { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } 
        public int PeriodMonths { get; set; }

        public string Status { get; set; } = "Draft";
    }
}
