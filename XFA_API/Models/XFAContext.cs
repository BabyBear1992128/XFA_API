using Microsoft.EntityFrameworkCore;
using XFA_API.Models;

namespace XFA_API.Models
{
    public class XFAContext : DbContext
    {
        public XFAContext(DbContextOptions<XFAContext> options) : base(options) 
        { 
            
        }

        public DbSet<DocumentModel> Documents { get; set; } = default!;
        public DbSet<ActionField> ActionFields { get; set; }
        public DbSet<InputField> InputFields { get; set; }
        public DbSet<ExportedFile> ExportedFiles { get; set; }

    }
}
