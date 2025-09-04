public class CertificateModel
{
    public string CertificateId { get; set; } = string.Empty;
    public string? ControllerId { get; set; }
    public string? FullName { get; set; }          // اسم المراقب
    public string? TypeName { get; set; }          // نوع الشهادة (نص واضح)
    public string? TypeId { get; set; }
    public string? CertificateTitle { get; set; }
    public string? IssuingAuthority { get; set; }
    public string? IssuingCountry { get; set; }
    public DateTime? IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Status { get; set; }
    public string? StatusReason { get; set; }
    public string? FilePath { get; set; }
    public string? Notes { get; set; }
}
