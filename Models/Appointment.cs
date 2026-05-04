using System;
using System.ComponentModel.DataAnnotations;

namespace SmartClinic.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Patient is required")]
        public int PatientId { get; set; }
        public Patient? Patient { get; set; }
        
        [Required(ErrorMessage = "Doctor is required")]
        public int DoctorId { get; set; }
        public User? Doctor { get; set; }
        
        [Required(ErrorMessage = "Appointment Date is required")]
        public DateTime AppointmentDate { get; set; }
        
        public string Status { get; set; } = "Scheduled"; // Scheduled, Completed, Cancelled
    }
}
