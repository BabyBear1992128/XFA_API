using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace XFA_API.Models
{
    public class DocumentRequest
    {

        public string Name { get; set; }

        public string Type { get; set; }
        public IFormFile PdfFile { get; set; }

        public string ValidationButton { get; set; } // Whether validation process is required

    }
}
