using System.Collections.Generic;
using WebApplication1.Models; // تأكد من أن موديل المشروع موجود هنا

namespace WebApplication1.ViewModels
{
    public class ProjectDetailViewModel
    {
        public Project Project { get; set; }
        public List<ProjectParticipantViewModel> Participants { get; set; }
        public List<string> Divisions { get; set; }
        public List<ProjectFileViewModel> Files { get; set; }

        public ProjectDetailViewModel()
        {
            Participants = new List<ProjectParticipantViewModel>();
            Divisions = new List<string>();
            Files = new List<ProjectFileViewModel>();
        }
    }
}