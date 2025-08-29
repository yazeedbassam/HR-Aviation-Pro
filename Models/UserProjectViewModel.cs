using System;

namespace WebApplication1.ViewModels
{
    public class UserProjectViewModel
    {
        public int ProjectId { get; set; }
        public string? ProjectName { get; set; }
        public string? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public string? Description { get; set; }

        public string? Location { get; set; }

    }
}