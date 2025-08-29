namespace WebApplication1.Models
{
    public class LicenseModel
    {
        public string LicenseId { get; set; }
        public string ControllerId { get; set; }
        public string LicenseType { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string PdfPath { get; set; }
        public string PhotoPath { get; set; }
        public string Range { get; set; }
        public string Note { get; set; }
        public DateTime? IssueDate { get; set; }
        public string FullName { get; set; }
        public string licensenumber { get; set; }
        public string Username { get; set; }

    }
}
