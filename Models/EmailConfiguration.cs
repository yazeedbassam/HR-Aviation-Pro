namespace WebApplication1.Models
{
    public class EmailConfiguration
    {
        public string SmtpServer { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
        public bool EnableSsl { get; set; } = true;
        public int Timeout { get; set; } = 30000;
        public int MaxRetries { get; set; } = 3;
        public int RetryDelay { get; set; } = 1000;
        public bool EnableLogging { get; set; } = true;
    }

    public class BulkEmailRequest
    {
        public List<int> SelectedIds { get; set; } = new List<int>();
        public string CustomMessage { get; set; } = string.Empty;
    }
} 