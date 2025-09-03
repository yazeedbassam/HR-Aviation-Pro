// File: Models/ObservationIndexViewModel.cs
using System.Collections.Generic;

namespace WebApplication1.Models
{
    public class ObservationIndexViewModel
    {
        public List<Observation> ControllerObservations { get; set; }
        public List<Observation> EmployeesAndOpsStaffObservations { get; set; }

        public ObservationIndexViewModel()
        {
            ControllerObservations = new List<Observation>();
            EmployeesAndOpsStaffObservations = new List<Observation>();
        }
    }
}