using System.ComponentModel.DataAnnotations;

namespace XFA_API.Models
{
    public class APIResponse
    {
        public APIResponse() 
        { 
            success = true;
            invoice_link = "";
            pdf_link = "";
            message = "";
        }

        [Required]
        public bool success { get; set; }
        [Required]
        public string invoice_link { get; set; }
        [Required]
        public string pdf_link { get; set; }
        [Required]
        public string message { get; set; }
    }
}
