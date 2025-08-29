// File: Models/ProjectViewModel.cs
using System;
using System.Collections.Generic;

namespace WebApplication1.Models
{
    /// <summary>
    /// Represents a single project card in the Index view.
    /// It holds the project's main data plus its related participants and divisions.
    /// </summary>
    public class ProjectViewModel
    {
        public Project Project { get; set; }
        public List<string> Participants { get; set; }
        public List<string> Divisions { get; set; }

        public ProjectViewModel()
        {
            Project = new Project();
            Participants = new List<string>();
            Divisions = new List<string>();
        }
    }
}