using System.Net.Http.Headers;
using XFA_API.Models;

namespace XFA_API.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly XFAContext _context;
        private readonly IWebHostEnvironment _env;

        public DocumentService(XFAContext socialDbContext, IWebHostEnvironment env)
        {
            _context = socialDbContext;
            _env = env;
        }
        public async Task<DocumentResponse> CreateDocumentAsync(DocumentRequest documentRequest, string filePath)
        {
            var doc = new DocumentModel
            {
                name = documentRequest.Name,
                type = documentRequest.Type,
                validation_button = documentRequest.ValidationButton,
                file_path = filePath,
            };

            var docEntry = await _context.Documents.AddAsync(doc);
            var saveResponse = await _context.SaveChangesAsync();
            if (saveResponse < 0)
            {
                return new DocumentResponse { Success = false, Error = "Issue while saving the post", ErrorCode = "CP01" };
            }
            var docEntity = docEntry.Entity;

            return new DocumentResponse { Success = true, Doc = doc };
        }

        public async Task<string> SaveDocumentFileAsync(DocumentRequest docRequest)
        {
            var uniqueFileName = Path.GetRandomFileName() + Path.GetFileName(docRequest.PdfFile.FileName);

            var folderName = Path.Combine("Resources", "Pdf");

            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

            var filePath = Path.Combine(pathToSave, uniqueFileName);

            //Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            //await docRequest.PdfFile.CopyToAsync(new FileStream(filePath, FileMode.Create));

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                docRequest.PdfFile.CopyTo(stream);
            }

            return filePath;
        }

        public async Task DownloadFileById(int Id)
        {
            //try
            //{
            //    var file = _context.Documents.Where(x => x.Id == Id).FirstOrDefault();

            //    var content = new System.IO.MemoryStream(file.FilePath);
            //    var path = Path.Combine(
            //       Directory.GetCurrentDirectory(), "FileDownloaded",
            //       file.Result.FileName);

            //    await CopyStream(content, path);
            //}
            //catch (Exception)
            //{
            //    throw;
            //}
        }

        public async Task CopyStream(Stream stream, string downloadPath)
        {
            using (var fileStream = new FileStream(downloadPath, FileMode.Create, FileAccess.Write))
            {
                await stream.CopyToAsync(fileStream);
            }
        }
    }
}
