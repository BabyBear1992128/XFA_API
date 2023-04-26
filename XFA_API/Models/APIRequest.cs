namespace XFA_API.Models
{
    public class APIRequest
    {
        public string Doc_type { get; set; }
        public Pdf[] Pdfs { get; set; }
        public Xml[] Xmls { get; set; }
        public Invoice Invoice { get; set; }
    }

    public class Pdf
    {
        public string Path { get; set; }
        public string Type { get; set; }
        public string Times { get; set; }
    }

    public class Xml
    {
        public string Path { get; set; }
        public string Value { get; set; }
    }

    public class Invoice
    {
        public string Valuta { get; set; }
        public string Client_Denumire { get; set; }
        public string Client_Tip { get; set; }
        public string Denumire { get; set; }
        public string UM { get; set; }
        public string NrProduse { get; set; }
        public string CotaTVA { get; set; }
        public string Client_CodUnic { get; set; }
        public string Client_NrRegCom { get; set; }
        public string Client_Judet { get; set; }
        public string Client_Localitate { get; set; }
        public string Client_Adresa { get; set; }
    }
}
