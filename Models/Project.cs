// File: Models/Project.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Project
    {
        public int ProjectId { get; set; }

        [Required]
        public string ProjectName { get; set; }
        public string? Description { get; set; }
        public string?Location { get; set; }
        public string? AssociatedEntity { get; set; }
        public string? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? FolderPath { get; set; }
    }
}
