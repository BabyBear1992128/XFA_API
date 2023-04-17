using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using XFA_API.Models;

namespace XFA_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExportedFilesController : ControllerBase
    {
        private readonly XFAContext _context;

        public ExportedFilesController(XFAContext context)
        {
            _context = context;
        }

        // GET: api/ExportedFiles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExportedFile>>> GetExportedFiles()
        {
          if (_context.ExportedFiles == null)
          {
              return NotFound();
          }
            return await _context.ExportedFiles.ToListAsync();
        }

        // GET: api/ExportedFiles/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ExportedFile>> GetExportedFile(long id)
        {
          if (_context.ExportedFiles == null)
          {
              return NotFound();
          }
            var exportedFile = await _context.ExportedFiles.FindAsync(id);

            if (exportedFile == null)
            {
                return NotFound();
            }

            return exportedFile;
        }

        // DELETE: api/ExportedFiles/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExportedFile(long id)
        {
            if (_context.ExportedFiles == null)
            {
                return NotFound();
            }
            var exportedFile = await _context.ExportedFiles.FindAsync(id);
            if (exportedFile == null)
            {
                return NotFound();
            }

            _context.ExportedFiles.Remove(exportedFile);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET
        // Download Exported PDF File
        [HttpGet("Download/{id}")]
        public async Task<IActionResult> DownloadExportFile(long id)
        {
            if (_context.ExportedFiles == null)
            {
                return NotFound();
            }

            var fileInfo = await _context.ExportedFiles.FindAsync(id);

            if (fileInfo == null)
            {
                return NotFound();
            }

            var filePath = fileInfo.Path;

            FileStream inFile = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            new FileExtensionContentTypeProvider().TryGetContentType(filePath, out var contentType);

            if(contentType == null)
            {
                contentType = "application/octet-stream";
            }

            var fileExtension = Path.GetExtension(filePath);

            FileStreamResult fileStreamResult = new FileStreamResult(inFile, contentType);
            fileStreamResult.FileDownloadName = Path.GetRandomFileName() + "_export" + fileExtension;

            return fileStreamResult;
        }

        private bool ExportedFileExists(long id)
        {
            return (_context.ExportedFiles?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
