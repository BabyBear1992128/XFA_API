namespace XFA_API.Models
{
    public class ActionFieldRequest
    {
        public long Id { get; set; }
        public ActionMap[] Actions { get; set; }
    }

    public class ActionMap
    {
        public string FieldPath { get; set; }
        public string Type { get; set; }
        public string Data { get; set; }
    }
}
