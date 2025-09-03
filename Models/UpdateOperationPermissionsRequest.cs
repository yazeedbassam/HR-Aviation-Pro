using System.Collections.Generic;

namespace WebApplication1.Models
{
    public class UpdateOperationPermissionsRequest
    {
        public int UserId { get; set; }
        public List<OperationPermissionUpdate> OperationPermissions { get; set; } = new List<OperationPermissionUpdate>();
    }
}