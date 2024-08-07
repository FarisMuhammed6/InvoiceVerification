using InvoiceVeMVC.DataContext;
using InvoiceVeMVC.Models;
using InvoiceVeMVC.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace InvoiceVeMVC.Controllers
{
    public class InvoiceController : Controller
    {
        private readonly OcrService _ocrService;
        private readonly InvoiceDbContext _dbcontext;

        public InvoiceController(OcrService ocrService, InvoiceDbContext dbcontext)
        {
            _ocrService = ocrService;
            _dbcontext = dbcontext;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadAndVerifyInvoice(IFormFile file, string contractName)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("File", "Invalid file.");
                return View("Index");
            }

            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            var filePath = Path.Combine(uploadsPath, file.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var extractedText = _ocrService.ExtractTextFromImage(filePath);
            var clientName = ExtractClientFromText(extractedText);
            var invoiceDate = ExtractInvoiceDate(extractedText);
            var amount = ExtractAmountFromText(extractedText);

            var invoice = new InvoiceViewModel
            {
                ClientName = clientName,
                InvoiceDate = invoiceDate,
                Amount = amount,
                IsValid = ValidateInvoice(clientName, invoiceDate, amount, contractName)
            };

            _dbcontext.Invoices.Add(invoice);
            await _dbcontext.SaveChangesAsync();

            ViewBag.Message = "Invoice uploaded and verified successfully.";
            return View("Index", invoice);
        }

        private string ExtractClientFromText(string text)
        {
            var clientPattern = @"Client:\s*(\w+)";
            var match = Regex.Match(text, clientPattern);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            throw new Exception("Client name not found.");
        }

        private DateTime ExtractInvoiceDate(string text)
        {
            var datePattern = @"\b\d{1,2}/\d{1,2}/\d{4}\b";
            var match = Regex.Match(text, datePattern);
            if (match.Success)
            {
                if (DateTime.TryParse(match.Value, out DateTime date))
                {
                    return date;
                }
            }
            throw new Exception("Date not found or invalid date format.");
        }

        private decimal ExtractAmountFromText(string text)
        {
            var amountPattern = @"(?:total amount due|amount|cost|price|fee|charge|payment).*?(\$?\d+(\.\d{1,2})?)";
            var match = Regex.Match(text, amountPattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var amountString = match.Groups[1].Value;
                amountString = amountString.Replace("$", "");

                if (decimal.TryParse(amountString, out decimal amount))
                {
                    return amount;
                }
            }
            throw new Exception("Total amount not found or invalid amount format.");
        }

        private bool ValidateInvoice(string clientName, DateTime invoiceDate, decimal amount, string contractName)
        {
            var contract = _dbcontext.Contracts
                .FirstOrDefault(c => c.ContractName == contractName && c.Amount >= amount);
            return contract != null;
        }
    }
}
