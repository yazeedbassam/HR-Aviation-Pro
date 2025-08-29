// File: Models/ProjectParticipant.cs
namespace WebApplication1.Models
{
    public class ProjectParticipant
    {
        public int ProjectParticipantId { get; set; }
        public int ProjectId { get; set; }
        public int? ControllerId { get; set; }
        public int? EmployeeId { get; set; }



    }
}