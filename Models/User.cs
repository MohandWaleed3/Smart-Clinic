using System.ComponentModel.DataAnnotations;

namespace SmartClinic.Models
{
    public class User
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }
        
        [Required(ErrorMessage = "Code is required")]
        public string Code { get; set; }
        
        [Required(ErrorMessage = "Role is required")]
        public string Role { get; set; } // Admin, Doctor, Reception, Patient
        
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
