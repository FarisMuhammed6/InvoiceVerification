using Tesseract;

namespace InvoiceVeMVC.Services
{
    public class OcrService
    {
        public string ExtractTextFromImage(string imagePath)
        {
            var ocrText = string.Empty;

            using (var engine = new TesseractEngine(@"./tessdata", "eng", Tesseract.EngineMode.Default))
            {
                using (var img = Pix.LoadFromFile(imagePath))
                {
                    using (var page = engine.Process(img))
                    {
                        ocrText = page.GetText();
                    }
                }
            }
            return ocrText;
        }
    }
}
