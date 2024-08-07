using InvoiceVeMVC.DataContext;
using InvoiceVeMVC.Models;
using InvoiceVeMVC.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace InvoiceVeMVC.Controllers
{
    public class ContractController : Controller
    {
        private readonly OcrService _ocrService;
        private readonly InvoiceDbContext _dbcontext;

        public ContractController(OcrService ocrService, InvoiceDbContext dbcontext)
        {
            _ocrService = ocrService;
            _dbcontext = dbcontext;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadContract(IFormFile file, string contractName)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "Invalid file.");
                return View("Index");
            }

            var uploadsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");

            if (!Directory.Exists(uploadsFolderPath))
            {
                Directory.CreateDirectory(uploadsFolderPath);
            }

            var filePath = Path.Combine(uploadsFolderPath, file.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var extractedText = _ocrService.ExtractTextFromImage(filePath);
            var clientName = ExtractClientFromText(extractedText);
            (DateTime startDate, DateTime endDate) = ExtractContractDates(extractedText);
            var amount = ExtractAmountFromText(extractedText);

            var contract = new Contract
            {
                ContractName = contractName,
                ClientName = clientName,
                StartDate = startDate,
                EndDate = endDate,
                Amount = amount
            };

            _dbcontext.Contracts.Add(contract);
            await _dbcontext.SaveChangesAsync();

            ViewBag.Message = "Contract uploaded successfully!";
            return View("Index");
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

        private static (DateTime startDate, DateTime endDate) ExtractContractDates(string text)
        {
            var datePattern = @"\b\d{1,2}/\d{1,2}/\d{4}\b";
            var matches = Regex.Matches(text, datePattern);

            if (matches.Count >= 2)
            {
                if (DateTime.TryParse(matches[0].Value, out DateTime startDate) &&
                    DateTime.TryParse(matches[matches.Count - 1].Value, out DateTime endDate))
                {
                    return (startDate, endDate);
                }
            }
            throw new Exception("Start date or end date not found or invalid date format.");
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
    }
}