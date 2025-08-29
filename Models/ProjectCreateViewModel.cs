using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class ProjectCreateViewModel
    {
        public Project Project { get; set; }

        // This single list will hold both controllers and employees for the dropdown
        public List<SelectListItem> AllParticipants { get; set; }
        public List<SelectListItem> AllDivisions { get; set; }

        // These will hold the IDs of the selected items from the form
        [Required(ErrorMessage = "Please select at least one participant.")]
        public List<string> SelectedParticipantIds { get; set; }

        [Required(ErrorMessage = "Please select at least one division.")]
        public List<int> SelectedDivisionIds { get; set; }

        public ProjectCreateViewModel()
        {
            Project = new Project();
            AllParticipants = new List<SelectListItem>();
            AllDivisions = new List<SelectListItem>();
            SelectedParticipantIds = new List<string>();
            SelectedDivisionIds = new List<int>();
        }
    }
}
