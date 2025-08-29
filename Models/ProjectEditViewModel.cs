using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WebApplication1.Models;

namespace WebApplication1.ViewModels
{
    public class ProjectEditViewModel
    {
        // We need the ID to know which project we are editing
        public int ProjectId { get; set; }

        public Project Project { get; set; }

        [Display(Name = "Participants")]
        public List<string> SelectedParticipantIds { get; set; }
        public List<SelectListItem> AllParticipants { get; set; }

        [Display(Name = "Divisions")]
        public List<int> SelectedDivisionIds { get; set; }
        public List<SelectListItem> AllDivisions { get; set; }

        public ProjectEditViewModel()
        {
            Project = new Project();
            SelectedParticipantIds = new List<string>();
            AllParticipants = new List<SelectListItem>();
            SelectedDivisionIds = new List<int>();
            AllDivisions = new List<SelectListItem>();
        }
    }
}
