// File: Models/ObservationIndexViewModel.cs
using System.Collections.Generic;

namespace WebApplication1.Models
{
    public class ObservationIndexViewModel
    {
        public List<Observation> ControllerObservations { get; set; }
        public List<Observation> AISObservations { get; set; }
        public List<Observation> CNSObservations { get; set; }
        public List<Observation> AFTNObservations { get; set; }
        public List<Observation> ATFMObservations { get; set; }
        public List<Observation> OpsStaffObservations { get; set; }

        public ObservationIndexViewModel()
        {
            ControllerObservations = new List<Observation>();
            AISObservations = new List<Observation>();
            CNSObservations = new List<Observation>();
            AFTNObservations = new List<Observation>();
            ATFMObservations = new List<Observation>();
            OpsStaffObservations = new List<Observation>();
        }
    }
}