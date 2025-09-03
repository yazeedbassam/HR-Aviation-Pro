using System.Collections.Generic;

namespace WebApplication1.Models
{
    public class UpdateOrganizationalPermissionsRequest
    {
        public int UserId { get; set; }
        public List<OrganizationalPermissionUpdate> OrganizationalPermissions { get; set; } = new List<OrganizationalPermissionUpdate>();
    }
}