// File: Models/ProjectIndexViewModel.cs
using System.Collections.Generic;

namespace WebApplication1.Models
{
    /// <summary>
    /// This is the main model for the Projects/Index.cshtml page.
    /// It holds the list of all projects to be displayed.
    /// </summary>
    public class ProjectIndexViewModel
    {
        public List<ProjectViewModel> Projects { get; set; }

        public ProjectIndexViewModel()
        {
            Projects = new List<ProjectViewModel>();
        }
    }
}
