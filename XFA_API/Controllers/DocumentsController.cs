using iText.Forms.Xfa;
using iText.Forms;
using iText.Kernel.Pdf;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Syncfusion.Pdf.Xfa;
using TallComponents.PDF;
using TallComponents.PDF.Annotations.Widgets;
using TallComponents.PDF.Forms.Fields;
using TallComponents.PDF.JavaScript;
using XFA_API.Models;
using XFA_API.Services;
using TallComponents.PDF.Actions;
using TallComponents.PDF.Forms.Data;

namespace XFA_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        private readonly XFAContext _context;
        private readonly IDocumentService _service;

        public DocumentsController(XFAContext context, IDocumentService service)
        {
            _context = context;
            _service = service;
        }

        // GET: api/Documents
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DocumentModel>>> GetDocument()
        {
            if (_context.Documents == null)
            {
                return NotFound();
            }
            return await _context.Documents.ToListAsync();
        }

        // GET: api/Documents/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DocumentModel>> GetDocument(long id)
        {
            if (_context.Documents == null)
            {
                return NotFound();
            }
            var document = await _context.Documents.FindAsync(id);

            if (document == null)
            {
                return NotFound();
            }

            return document;
        }

        // GET : api/Documents/AllPath/5
        [HttpGet("AllApth/{id}")]
        public async Task<ActionResult<IEnumerable<string>>> GetDocumentAllPath(long id)
        {
            if (_context.Documents == null)
            {
                return NotFound();
            }
            var document = await _context.Documents.FindAsync(id);

            if (document == null)
            {
                return NotFound();
            }

            var FilePath = document.file_path;

            FileStream inFile = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            Document doc = new Document(inFile);

            List<string> paths = doc.Fields.FullNames.ToList();

            return paths;
        }

        // Export XFA Data to XML
        [HttpGet("ExportXFAData/{id}")]
        public async Task<IActionResult> ExportXFAData(long id)
        {
            if (_context.Documents == null)
            {
                return NotFound();
            }
            var document = await _context.Documents.FindAsync(id);

            if (document == null)
            {
                return NotFound();
            }

            var filePath = document.file_path;

            FileStream inFile = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            //
            PdfLoadedXfaDocument ldoc = new PdfLoadedXfaDocument(inFile);

            PdfLoadedXfaForm lform = ldoc.XfaForm;

            MemoryStream ms = new MemoryStream();

            lform.ExportXfaData(ms);

            ms.Position = 0;

            FileStreamResult fileStreamResult = new FileStreamResult(ms, "application/xml");
            fileStreamResult.FileDownloadName = Path.GetRandomFileName() + "_export.xml";

            return fileStreamResult;
        }

        // GET
        [HttpGet("MergeXDPAndXFA/{id}")]
        public async Task<IActionResult> MergeXDPAndXFA(long id)
        {
            if (_context.Documents == null)
            {
                return NotFound();
            }
            var document = await _context.Documents.FindAsync(id);

            if (document == null)
            {
                return NotFound();
            }

            var filePath = document.file_path;

            manipulatePdf(filePath, "E:/merged.pdf");

            return Ok();
        }

        // GET
        [HttpPost("GenAndValid")]
        public async Task<IActionResult> GetAndValidDocument(ActionFieldRequest actionFieldRequests) 
        {
            if (_context.Documents == null)
            {
                return NotFound();
            }

            var id = actionFieldRequests.Id;

            var document = await _context.Documents.FindAsync(id);

            if (document == null)
            {
                return NotFound();
            }

            var filePath = document.file_path;

            //

            //

            FileStream inFile = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            Document doc = new Document(inFile);

            doc.Fields.Changed += Fields_Changed;


            // Action
            foreach (var action in actionFieldRequests.Actions)
            {
                switch (action.Type)
                {
                    case "button":
                        var field1 = doc.Fields[action.FieldPath];

                        if(field1 != null && field1 is PushButtonField)
                        {
                            var buttonField = field1 as PushButtonField;

                            if (buttonField != null) 
                            {
                                buttonField.XfaInfo.ClickActions.Execute();
                            }
                        }
                        break;
                    case "radio":
                        var field2 = doc.Fields[action.FieldPath];

                        if (field2 != null && field2 is RadioButtonField)
                        {
                            var radioField = field2 as RadioButtonField;

                            if (radioField != null)
                            {
                                radioField.Value = action.Data;
                            }
                        }
                        break;
                    default: break;
                }
            }

            //PushButtonField buttonField = doc.Fields["form1[0].btnDoc[0].btnValid[0]"] as PushButtonField;

            //buttonField.XfaInfo.ClickActions.Execute();

            //RadioButtonField radioField1 = doc.Fields["form1[0].s211[0].date[0].Cat[0].cat[0].slct[0]"] as RadioButtonField;
            //RadioButtonField radioField2 = doc.Fields["form1[0].s211[0].date[0].Cat[0].cat[0].chirie[0]"] as RadioButtonField;

            //radioField1.Value = "1";

            // Export to XDP
            //XdpFormData xdp = doc.Export(SubmitFormat.Xdp, false) as XdpFormData;
            //xdp.Path = filePath;

            //FileStream xdpFile = new FileStream("E:/Purchase Order_data.xdp", FileMode.Create, FileAccess.Write);

            //xdp.Write(xdpFile);

            //xdpFile.Close();

            //doc.ScriptBehavior = ScriptBehavior.Format;

            MemoryStream ms = new MemoryStream();

            doc.Write(ms);

            ms.Position = 0;

            FileStreamResult fileStreamResult = new FileStreamResult(ms, "application/pdf");
            fileStreamResult.FileDownloadName = Path.GetRandomFileName() + "_export.pdf";

            inFile.Close();

            return fileStreamResult;
        }

        // PUT: api/Documents/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDocument(long id, DocumentModel document)
        {
            if (id != document.id)
            {
                return BadRequest();
            }

            _context.Entry(document).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DocumentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Documents
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<IActionResult> PostDocument([FromForm] DocumentRequest documentRequest)
        {
            if (documentRequest == null)
            {
                return BadRequest(new DocumentResponse { Success = false, ErrorCode = "S01", Error = "Invalid post request" });
            }

            if (string.IsNullOrEmpty(Request.GetMultipartBoundary()))
            {
                return BadRequest(new DocumentResponse { Success = false, ErrorCode = "S02", Error = "Invalid post header" });
            }

            string filePath = "";
            if (documentRequest.PdfFile != null)
            {
                filePath = await _service.SaveDocumentFileAsync(documentRequest);
            }

            var docResponse = await _service.CreateDocumentAsync(documentRequest, filePath);
            if (!docResponse.Success)
            {
                return NotFound(docResponse);
            }

            return Ok(docResponse.Doc);
        }

        // DELETE: api/Documents/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocument(long id)
        {
            if (_context.Documents == null)
            {
                return NotFound();
            }
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }

            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DocumentExists(long id)
        {
            return (_context.Documents?.Any(e => e.id == id)).GetValueOrDefault();
        }

        private static void Fields_Changed(FieldCollection sender, FieldsChangedEventArgs e)
        {
            // note that after invoking the 'Add Item' buttons Click action, 6 new fields were added to the collection
            //System.Diagnostics.Debug.Assert(e.Added.Length == 6);
        }

        private static void manipulatePdf(String src, String dest)
        {
            FileStream xml = new FileStream("E:/export.xml", FileMode.Open);

            PdfReader reader = new PdfReader(src);
            PdfDocument pdfDoc = new PdfDocument(reader, new PdfWriter(dest), new StampingProperties().UseAppendMode());
            PdfAcroForm form = PdfAcroForm.GetAcroForm(pdfDoc, true);
            XfaForm xfa = form.GetXfaForm();
            xfa.FillXfaForm(xml);
            xfa.Write(pdfDoc);
            pdfDoc.Close();
        }
    }
}
