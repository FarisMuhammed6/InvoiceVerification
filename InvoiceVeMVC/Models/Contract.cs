namespace InvoiceVeMVC.Models
{
    public class Contract
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Amount { get; set; }
        public string? ClientName { get; set; }
        public string? ContractName { get; set; }
    }
}
