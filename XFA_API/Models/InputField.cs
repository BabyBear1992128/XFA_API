using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace XFA_API.Models
{
    [Table("input_fields")]
    public class InputField
    {
        [Key, Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }
        public string name { get; set; }
        public string identifier { get; set; }

        public long document_id { get; set; }
    }
}
