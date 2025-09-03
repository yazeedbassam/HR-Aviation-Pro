// File: Models/Observation.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace WebApplication1.Models
{
    public class Observation
    {
        public int ObservationId { get; set; }
        public int? ObservationNo { get; set; }
        [Required(ErrorMessage = "License Number is required.")]
        public string? LicenseNumber { get; set; }
        public string? Notes { get; set; }
        public string? FilePath { get; set; }
        public string? FlightNo { get; set; }
        public int? DurationDays { get; set; }
        public string? TravelCountry { get; set; }
        public DateTime? DepartDate { get; set; }
        public DateTime? ReturnDate { get; set; }

        public int? ControllerId { get; set; }
        public string? ControllerName { get; set; }

        public int? EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeDepartment { get; set; }

        [NotMapped]
        public string? PersonName => ControllerName ?? EmployeeName;

        [NotMapped]
        public IFormFile? UploadFile { get; set; }

        [NotMapped]
        public int TotalPreviousTrips { get; set; }
    }
}