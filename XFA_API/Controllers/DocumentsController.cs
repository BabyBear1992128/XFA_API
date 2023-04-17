using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Syncfusion.Pdf.Xfa;
using TallComponents.PDF;
using TallComponents.PDF.Actions;
using TallComponents.PDF.Annotations.Widgets;
using TallComponents.PDF.Forms.Data;
using TallComponents.PDF.Forms.Fields;
using TallComponents.PDF.JavaScript;
using XFA_API.Models;
using XFA_API.Services;
using TextField = TallComponents.PDF.Forms.Fields.TextField;
using System.Xml.Linq;
using System.Xml;
using iTextSharp.text.pdf;
using System.Text;

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

            // Syncfusion
            PdfLoadedXfaDocument loadedDocument = new PdfLoadedXfaDocument(inFile);

            PdfLoadedXfaForm loadedForm = loadedDocument.XfaForm;

            // iText

            // Aspose


            return paths;
        }

        // Export XFA Data to XML
        [HttpGet("ExportXFAData1/{id}")]
        public async Task<IActionResult> ExportXFAData1(long id)
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

            //
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

        // Export XFA Data to XML
        //[HttpGet("ExportXFAData2/{id}")]
        //public async Task<IActionResult> ExportXFAData2(long id)
        //{
        //    if (_context.ExportedFiles == null)
        //    {
        //        return NotFound();
        //    }
        //    var exportFile = await _context.ExportedFiles.FindAsync(id);

        //    if (exportFile == null)
        //    {
        //        return NotFound();
        //    }

        //    var filePath = exportFile.Path;

        //    FileStream inFile = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

        //    //
        //    PdfLoadedXfaDocument ldoc = new PdfLoadedXfaDocument(inFile);

        //    PdfLoadedXfaForm lform = ldoc.XfaForm;

        //    MemoryStream ms = new MemoryStream();

        //    lform.ExportXfaData(ms);

        //    ms.Position = 0;

        //    FileStreamResult fileStreamResult = new FileStreamResult(ms, "application/xml");
        //    fileStreamResult.FileDownloadName = Path.GetRandomFileName() + "_export.xml";

        //    return fileStreamResult;
        //}

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

            mergeXFA(filePath, "E:/export.xml", "E:/merged.pdf");
            //mergeXFA1(filePath);

            return Ok();
        }

        // POST
        [HttpPost("ValidationDoc")]
        public async Task<ActionResult<string>> ValidationDoc([FromForm] ValidationRequest vRequest)
        {
            var docId = vRequest.DocumentId;

            if(_context.Documents == null)
            {
                return NotFound();
            }

            var doc = await _context.Documents.FindAsync(docId);

            if(doc == null)
            {
                return NotFound();
            }

            FileStream inFile = new FileStream(doc.file_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            Document document = new Document(inFile);

            document.Fields.Changed += Fields_Changed;

            // Find Validation Button Path
            var validationButtonPath = doc.validation_button;

            // Find Validation Button
            PushButtonField buttonField = document.Fields[validationButtonPath] as PushButtonField;

            if(buttonField == null)
            {
                return NotFound(nameof(validationButtonPath));
            }

            // Click Validation Button
            buttonField.XfaInfo.ClickActions.Execute();

            // Export file
            var uniqueFileName = DateTimeOffset.Now.ToUnixTimeMilliseconds() + ".pdf";

            var folderName = Path.Combine("Resources", "Pdf", "Export");

            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

            var exportPath = Path.Combine(pathToSave, uniqueFileName);

            Directory.CreateDirectory(Path.GetDirectoryName(exportPath));

            FileStream exportFile = new FileStream(exportPath, FileMode.Create, FileAccess.Write);

            document.Write(exportFile);

            exportFile.Close();

            // Save to context
            ExportedFile exportedFile = new ExportedFile
            {
                Path = exportPath,
            };

            await _service.SaveExportedFileAsync(exportedFile);

            //
            return "/api/ExportedFiles/Download/" + exportedFile.Id;
        }

        // POST
        [HttpPost("GenerationDoc")]
        public async Task<ActionResult<string>> GenerationDoc(ActionFieldRequest actionFieldRequests) 
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

            FileStream inFile = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            Document doc = new Document(inFile);

            doc.ScriptBehavior = ScriptBehavior.Format;

            //doc.Fields.Changed += Fields_Changed;

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
                    case "checkbox":
                        var field3 = doc.Fields[action.FieldPath];

                        if (field3 != null && field3 is CheckBoxField)
                        {
                            var checkField = field3 as CheckBoxField;

                            if (checkField != null)
                            {
                                if(action.Data == "true")
                                {
                                    checkField.CheckBoxValue = CheckState.On;
                                }
                                else
                                {
                                    checkField.CheckBoxValue = CheckState.Off;
                                }
                            }
                        }
                        break;
                    default: break;
                }
            }

            // Export to XDP
            XdpFormData xdp = doc.Export(SubmitFormat.Xdp, false) as XdpFormData;
            //xdp.Path = filePath;

            var xdpPath = Path.Combine("Resources", "Pdf", "Xdp", DateTimeOffset.Now.ToUnixTimeMilliseconds() + ".xdp");
            var xmlPath = Path.Combine("Resources", "Pdf", "Xml", DateTimeOffset.Now.ToUnixTimeMilliseconds() + ".xml");

            Directory.CreateDirectory(Path.GetDirectoryName(xdpPath));
            Directory.CreateDirectory(Path.GetDirectoryName(xmlPath));

            using (FileStream xdpFile = new FileStream(xdpPath, FileMode.Create, FileAccess.Write))
            {
                xdp.Write(xdpFile);
            }

            //// Generate XML from XDP
            //XmlDocument xmlDoc1 = new XmlDocument();
            //xmlDoc1.Load(xdpPath);
            //var xfaData = xmlDoc1.LastChild.FirstChild.OuterXml;

            //XmlDocument xdoc = new XmlDocument();
            //xdoc.LoadXml(xfaData);
            //xdoc.Save(xmlPath);

            //// Merge XML and Blank File
            //var uniqueFileName = DateTimeOffset.Now.ToUnixTimeMilliseconds() + ".pdf";

            //var folderName = Path.Combine("Resources", "Pdf", "Export");

            //var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

            //var exportPath = Path.Combine(pathToSave, uniqueFileName);

            //Directory.CreateDirectory(Path.GetDirectoryName(exportPath));

            //mergeXFA(filePath, xmlPath, exportPath);

            // Save to context
            ExportedFile exportedFile = new ExportedFile
            {
                Path = xdpPath,
            };

            await _service.SaveExportedFileAsync(exportedFile);

            //
            return "/api/ExportedFiles/Download/" + exportedFile.Id;
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
        
        // Merge XML and Blank with iTextSharp
        private static void mergeXFA(String sourceXfaPath, String exportXfaXml, String exportPdf)
        {
            using (FileStream pdf = new FileStream(sourceXfaPath, FileMode.Open))
            using (FileStream xml = new FileStream(exportXfaXml, FileMode.Open))
            using (FileStream filledPdf = new FileStream(exportPdf, FileMode.Create))
            {
                iTextSharp.text.pdf.PdfReader pdfReader = new iTextSharp.text.pdf.PdfReader(pdf);

                try
                {
                    PdfStamper stamper = new PdfStamper(pdfReader, filledPdf, '\0', true);

                    stamper.AcroFields.Xfa.FillXfaForm(xml);
                    stamper.Close();
                    pdfReader.Close();
                }
                catch(NullReferenceException ex)
                {
                    Console.WriteLine(ex.ToString());
                }

                
            }
        }

        // Merge XDP and Blank with PDFKit.NET 5
        private static void mergeXFA1(string xfaFilePath)
        {
            using (FileStream inFile = new FileStream(xfaFilePath, FileMode.Open, FileAccess.Read))
            {
                // open 
                Document document = new Document(inFile);

                // import
                using (FileStream inXdp = new FileStream("E:/Purchase Order_data.xdp", FileMode.Open, FileAccess.Read))
                {
                    XdpFormData xdpData = new XdpFormData(inXdp);
                    document.Import(xdpData);
                }

                // save
                using (FileStream outFile = new FileStream("E:/Purchase Order Filled.pdf", FileMode.Create, FileAccess.Write))
                {
                    document.Write(outFile);
                }
            }
        }

        // Extract as XML with iText7
        protected static void ExportXMLFromPDF(string sourcePath)
        {
            iText.Kernel.Pdf.PdfDocument pdf = new iText.Kernel.Pdf.PdfDocument(new iText.Kernel.Pdf.PdfReader(sourcePath));
            iText.Forms.PdfAcroForm form = iText.Forms.PdfAcroForm.GetAcroForm(pdf, true);
            iText.Forms.Xfa.XfaForm xfa = form.GetXfaForm();
            XDocument doc = xfa.GetDomDocument();

            // save
            using (FileStream outFile = new FileStream("E:/asdfasdfasdf.xml", FileMode.Create, FileAccess.Write))
            {
                doc.Save(outFile);
            }
        }


        private static void FlattenPDF(string filePath)
        {
            using (FileStream fileIn = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                Document form = new Document(fileIn);

                // activate the javascript engine, so format actions will be executed.
                form.ScriptBehavior = ScriptBehavior.Format;

                // flatten all form-data with the current value, except text-field field-name.
                foreach (Field field in form.Fields)
                {
                    TextField textField = field as TextField;
                    if (textField != null)
                    {
                        textField.Value = textField.FullName;
                    }

                    foreach (Widget widget in field.Widgets)
                    {
                        widget.Persistency = WidgetPersistency.Flatten;
                    }
                }

                // write flattened form back to disk
                using (FileStream fileOut = new FileStream("E:/xxx.pdf", FileMode.Create, FileAccess.Write))
                {
                    form.Write(fileOut);
                }
            }
        }
    }
}
