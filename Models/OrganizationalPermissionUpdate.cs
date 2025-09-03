namespace WebApplication1.Models
{
    public class OrganizationalPermissionUpdate
    {
        public string PermissionType { get; set; } = string.Empty;
        public int EntityId { get; set; }
        public string EntityName { get; set; } = string.Empty;
        public bool CanView { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanCreate { get; set; }
    }
}