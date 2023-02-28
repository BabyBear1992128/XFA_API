using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.CodeAnalysis;

namespace XFA_API.Models
{
    [Table("action_field")]
    public class ActionField
    {
        [Key, Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }
        public string name { get; set; }
        public string identifier { get; set; }

        public long document_id { get; set; }
    }
}
