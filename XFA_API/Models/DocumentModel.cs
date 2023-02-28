using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XFA_API.Models
{
    [Table("documents")]
    public class DocumentModel
    {
        [Key, Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }

        public string name { get; set; }

        public string type { get; set; }
        public string file_path { get; set; } // PDF file stored as binary data

        public string validation_button { get; set; } // Whether validation process is required

        // Fields for PDF form generation and validation
        public ICollection<ActionField> action_fields { get; set; }

        // Text fields found in the document
        public ICollection<InputField> input_fields { get; set; }
    }
}
