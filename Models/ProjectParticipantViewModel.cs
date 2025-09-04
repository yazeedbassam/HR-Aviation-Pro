namespace WebApplication1.ViewModels
{
    public class ProjectParticipantViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // سيكون "Controller" أو "Employee"
        public string AvatarText { get; set; } = string.Empty;
        public string AvatarCssClass { get; set; } = string.Empty;
        public string? Role { get; set; }
    }
}