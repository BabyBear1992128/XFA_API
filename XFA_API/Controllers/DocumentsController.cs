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
using Microsoft.IdentityModel.Tokens;
using MySqlX.XDevAPI;
using Newtonsoft.Json.Linq;
using static System.Net.Mime.MediaTypeNames;
using System.Text.Json;
using NuGet.Protocol;
using System.Security.Cryptography;

namespace XFA_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        private readonly XFAContext _context;
        private readonly IDocumentService _service;
        private static readonly HttpClient client = new HttpClient();

        private string COMPANY_CUI = "42906264";
        private string PRIVATE_KEY = "ACAF8003CB2B303F6A6409E762DB9D20";
        private string INVOCE_RELEASE_ENDPOINT = "https://testapp.fgo.ro/publicws/factura/emitere";
        private string PDFKIT_API_ENDPOINT = "http://localhost:8000/api/Documents/GenerationDoc";
        private string PLATFORM_URL = "http://89.117.54.26";

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
                if (action == null) continue;

                switch (action.Type)
                {
                    case "button":
                        var field1 = doc.Fields[action.FieldPath];

                        if(field1 != null && field1 is PushButtonField)
                        {
                            var buttonField = field1 as PushButtonField;

                            if (buttonField != null) 
                            {
                                var times = Int32.Parse(action.Data);

                                if(times > 1)
                                {
                                    for(var i = 0; i < times; i++)
                                    {
                                        buttonField.XfaInfo.ClickActions.Execute();
                                    }
                                }
                                else
                                {
                                    buttonField.XfaInfo.ClickActions.Execute();
                                }
                                
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
                                checkField.CheckBoxValue = CheckState.On;
                            }
                        }
                        break;
                    case "text":
                        var field4 = doc.Fields[action.FieldPath];

                        if (field4 != null && field4 is TextField)
                        {
                            var textField = field4 as TextField;

                            if (textField != null)
                            {
                                if (!action.Data.IsNullOrEmpty())
                                {
                                    textField.Value = action.Data;
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

            // Generate XML from XDP
            XmlDocument xmlDoc1 = new XmlDocument();
            xmlDoc1.Load(xdpPath);
            var xfaData = xmlDoc1.LastChild.FirstChild.OuterXml;

            XmlDocument xdoc = new XmlDocument();
            xdoc.LoadXml(xfaData);
            xdoc.Save(xmlPath);

            // Merge XML and Blank File
            var uniqueFileName = DateTimeOffset.Now.ToUnixTimeMilliseconds() + ".pdf";

            var folderName = Path.Combine("Resources", "Pdf", "Export");

            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

            var exportPath = Path.Combine(pathToSave, uniqueFileName);

            Directory.CreateDirectory(Path.GetDirectoryName(exportPath));

            mergeXFA(filePath, xmlPath, exportPath);

            // Save to context
            ExportedFile exportedFile = new ExportedFile
            {
                //Path = xdpPath,
                Path = exportPath,
            };

            await _service.SaveExportedFileAsync(exportedFile);

            //
            return "/api/ExportedFiles/Download/" + exportedFile.Id;
        }

        [HttpPost("generate")]
        public async Task<ActionResult<APIResponse>> MainEndpoint(APIRequest apiRequest)
        {
            if (_context == null)
            {
                return NotFound();
            }

            if (apiRequest == null || apiRequest.Doc_type == null)
            {
                return NotFound();
            }

            var pdfRequest = new ActionFieldRequest();
            pdfRequest.Id = long.Parse(apiRequest.Doc_type);

            var actions = new List<ActionMap>();

            if (!apiRequest.Pdfs.IsNullOrEmpty())
            {
                var pdfs = apiRequest.Pdfs;
                foreach (var pdf in pdfs)
                {
                    var actionMap = new ActionMap();
                    actionMap.FieldPath = pdf.Path;
                    actionMap.Type = pdf.Type;
                    actionMap.Data = pdf.Times;

                    actions.Add(actionMap);
                }
            }

            if (!apiRequest.Xmls.IsNullOrEmpty())
            {
                foreach (var xml in apiRequest.Xmls)
                {
                    var actionMap = new ActionMap();
                    actionMap.FieldPath = xml.Path;
                    actionMap.Type = "text";
                    actionMap.Data = xml.Value;

                    actions.Add(actionMap);
                }
            }

            pdfRequest.Actions = actions.ToArray();

            // Extract Data to send request to PDF Kit module
            var returnResult = new APIResponse();
            returnResult.success = true;
            returnResult.message = "";

            // Send and receive from Invoice API
            var returnString = await GeneratePDF(pdfRequest);


            returnResult.pdf_link = returnString;

            Console.WriteLine(">>>>>>>>>>>>>>>>>>" + apiRequest.Doc_type);
            Console.WriteLine(">>>>>>>>>>>>>>>>>>" + returnString);

            // Extract Data to send request to Invoice API

            // Make request to send to Invoice API
            if (!apiRequest.Invoice.Valuta.IsNullOrEmpty())
            {
                var values = new Dictionary<string, string>
                {
                    { "CodUnic", COMPANY_CUI },
                    { "Hash", GenerateHash(apiRequest.Invoice.Client_CodUnic)},
                    { "Valuta", apiRequest.Invoice.Valuta.IsNullOrEmpty() ? "RON" : apiRequest.Invoice.Valuta },
                    { "TipFactura", apiRequest.Invoice.Client_Tip.IsNullOrEmpty() ? "Factura" : apiRequest.Invoice.Client_Tip },
                    { "Serie", "test series" },
                    { "Client[Denumire]", apiRequest.Invoice.Client_CodUnic },
                    { "Client[CodUnic]", "" }, //
                    { "Client[NrRegCom]", "" }, //
                    { "Client[Judet]", "" }, //
                    { "Client[Localitate]", "" }, //
                    { "Client[Adresa]", "" }, //
                    { "Client[Tip]", "PF" }, //
                    { "Continut[0][Denumire]", apiRequest.Invoice.Denumire.IsNullOrEmpty() ? "COMPLETARE DECLARATIE UNICA" : apiRequest.Invoice.Denumire },
                    { "Continut[0][UM]", apiRequest.Invoice.UM.IsNullOrEmpty() ? "BUC" : apiRequest.Invoice.UM },
                    { "Continut[0][NrProduse]", apiRequest.Invoice.NrProduse.IsNullOrEmpty() ? "2222.33" : apiRequest.Invoice.NrProduse },
                    { "Continut[0][CotaTVA]", apiRequest.Invoice.CotaTVA.IsNullOrEmpty() ? "19" : apiRequest.Invoice.CotaTVA },
                    { "Continut[0][PretUnitar]", apiRequest.Invoice.PretUnitar.IsNullOrEmpty() ? "22.00" : apiRequest.Invoice.PretUnitar },
                    { "PlatformaUrl", PLATFORM_URL },
                };

                Console.WriteLine(">>>>>>>>>>>>>>>>>>" + values.ToString());

                var content = new FormUrlEncodedContent(values);

                // Send and receive from Invoice API
                var response = await client.PostAsync(INVOCE_RELEASE_ENDPOINT, content);

                var responseString = await response.Content.ReadAsStringAsync();

                JObject json = JObject.Parse(responseString);

                Console.WriteLine(">>>>>>>>>>>>>>>>>>" + json.ToString());

                if (json.GetBoolean("Success").Value)
                {
                    var factura = json.GetValue("Factura");

                    var numar = factura.Value<string>("Numar");
                    var serie = factura.Value<string>("Serie");
                    var link = factura.Value<string>("Link");

                    returnResult.invoice_link = link;
                }
                else
                {
                    var message = json.Value<string>("Message");

                    returnResult.message = message;
                }
            }
            

            return returnResult;
        }

        private async Task<string> GeneratePDF(ActionFieldRequest actionFieldRequests)
        {
            var id = actionFieldRequests.Id;

            var document = await _context.Documents.FindAsync(id);

            if (document == null)
            {
                return "";
            }

            var filePath = document.file_path;

            FileStream inFile = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            Document doc = new Document(inFile);

            doc.ScriptBehavior = ScriptBehavior.Format;

            //doc.Fields.Changed += Fields_Changed;

            // Action
            foreach (var action in actionFieldRequests.Actions)
            {
                if (action == null) continue;

                switch (action.Type)
                {
                    case "button":
                        var field1 = doc.Fields[action.FieldPath];

                        if (field1 != null && field1 is PushButtonField)
                        {
                            var buttonField = field1 as PushButtonField;

                            if (buttonField != null)
                            {
                                var times = Int32.Parse(action.Data);

                                if (times > 1)
                                {
                                    for (var i = 0; i < times; i++)
                                    {
                                        buttonField.XfaInfo.ClickActions.Execute();
                                    }
                                }
                                else
                                {
                                    buttonField.XfaInfo.ClickActions.Execute();
                                }

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
                                checkField.CheckBoxValue = CheckState.On;
                            }
                        }
                        break;
                    case "text":
                        var field4 = doc.Fields[action.FieldPath];

                        if (field4 != null && field4 is TextField)
                        {
                            var textField = field4 as TextField;

                            if (textField != null)
                            {
                                if (!action.Data.IsNullOrEmpty())
                                {
                                    textField.Value = action.Data;
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

            // Generate XML from XDP
            XmlDocument xmlDoc1 = new XmlDocument();
            xmlDoc1.Load(xdpPath);
            var xfaData = xmlDoc1.LastChild.FirstChild.OuterXml;

            XmlDocument xdoc = new XmlDocument();
            xdoc.LoadXml(xfaData);
            xdoc.Save(xmlPath);

            // Merge XML and Blank File
            var uniqueFileName = DateTimeOffset.Now.ToUnixTimeMilliseconds() + ".pdf";

            var folderName = Path.Combine("Resources", "Pdf", "Export");

            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

            var exportPath = Path.Combine(pathToSave, uniqueFileName);

            Directory.CreateDirectory(Path.GetDirectoryName(exportPath));

            mergeXFA(filePath, xmlPath, exportPath);

            // Save to context
            ExportedFile exportedFile = new ExportedFile
            {
                //Path = xdpPath,
                Path = exportPath,
            };

            await _service.SaveExportedFileAsync(exportedFile);

            //

            return "http://89.117.54.26/api/ExportedFiles/Download/" + exportedFile.Id;
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

                if (pdfReader != null && filledPdf != null)
                {
                    PdfStamper stamper = new(pdfReader, filledPdf, '\0', true);

                    if (stamper != null)
                    {
                        if (xml != null)
                        {
                            stamper.AcroFields.Xfa.FillXfaForm(xml);
                            stamper.Close();
                        }
                        pdfReader.Close();
                    }
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



        // Generate HASH Code for send request to Invoice API
        private string GenerateHash(string input)
        {
            var temp = COMPANY_CUI + PRIVATE_KEY + input;

            return EncryptSHA1(temp);
        }

        //
        private string EncryptSHA1(string input)
        {
            using (SHA1 sha1Hash = SHA1.Create())
            {
                //From String to byte array
                byte[] sourceBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = sha1Hash.ComputeHash(sourceBytes);
                string hash = BitConverter.ToString(hashBytes).Replace("-", String.Empty);

                return hash;
            }
        }
    }
}
