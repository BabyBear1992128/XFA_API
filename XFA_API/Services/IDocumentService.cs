using XFA_API.Models;

namespace XFA_API.Services
{
    public interface IDocumentService
    {
        Task<string> SaveDocumentFileAsync(DocumentRequest documentRequest);

        Task<bool> SaveExportedFileAsync(ExportedFile exportedFile);


        Task<DocumentResponse> CreateDocumentAsync(DocumentRequest documentRequest, String filePath);
    }
}
