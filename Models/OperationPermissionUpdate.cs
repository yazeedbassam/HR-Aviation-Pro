namespace WebApplication1.Models
{
    public class OperationPermissionUpdate
    {
        public string EntityType { get; set; } = string.Empty;
        public string OperationType { get; set; } = string.Empty;
        public bool IsAllowed { get; set; }
        public string Scope { get; set; } = "All";
        public int? ScopeId { get; set; }
    }
}