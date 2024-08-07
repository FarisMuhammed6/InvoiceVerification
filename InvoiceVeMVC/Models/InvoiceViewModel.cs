namespace InvoiceVeMVC.Models
{
    public class InvoiceViewModel
    {
        public int Id { get; set; }
        public string? ClientName { get; set; }
        public DateTime InvoiceDate { get; set; }
        public decimal Amount { get; set; }
        public bool IsValid { get; set; }
    }
}
