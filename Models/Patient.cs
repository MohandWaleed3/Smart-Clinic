using System;
using System.ComponentModel.DataAnnotations;

namespace SmartClinic.Models
{
    public class Patient
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }
        
        [Required(ErrorMessage = "Code is required")]
        public string Code { get; set; }
        
        [Required(ErrorMessage = "Date of Birth is required")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }
        
        public string Phone { get; set; }
    }
}
