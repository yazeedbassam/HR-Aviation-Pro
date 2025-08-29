namespace WebApplication1.ViewModels
{
    public class ProjectParticipantViewModel
    {
        public string Name { get; set; }
        public string Type { get; set; } // سيكون "Controller" أو "Employee"
        public string AvatarText { get; set; }
        public string AvatarCssClass { get; set; }
        public string? Role { get; set; }
    }
}