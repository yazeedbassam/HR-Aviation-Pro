using WebApplication1.Models;
using System.Collections.Generic;

namespace WebApplication1.ViewModels
{
    public class ProfileProjectViewModel
    {
        public Project Project { get; set; }
        public List<string> Participants { get; set; }
        public List<string> Divisions { get; set; }

        public ProfileProjectViewModel()
        {
            Project = new Project();
            Participants = new List<string>();
            Divisions = new List<string>();
        }
    }
}