namespace XFA_API.Models
{
    public class ValidationRequest
    {
        public string RequestType { get; set; }
        public IFormFile PdfFile { get; set; }
        public long DocumentId { get; set; }
    }
}
