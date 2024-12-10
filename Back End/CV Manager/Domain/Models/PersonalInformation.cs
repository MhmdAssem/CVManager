using System.ComponentModel.DataAnnotations;

namespace CV_Manager.Domain.Models
{
    /// <summary>
    /// Represents personal information for a CV
    /// </summary>
    public class PersonalInformation
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Full name is required")]
        public string FullName { get; set; }
        
        public string CityName { get; set; }
        
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; }
        
        [Required(ErrorMessage = "Mobile number is required")]
        [RegularExpression(@"^[0-9]*$", ErrorMessage = "Mobile number must contain only numbers")]
        public string MobileNumber { get; set; }
    }
}