// File: Models/ProjectPhase.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class ProjectPhase
    {
        public int PhaseId { get; set; }
        public int ProjectId { get; set; }
        [Required]
        public string PhaseName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
    }
}