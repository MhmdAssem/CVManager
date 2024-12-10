using System.ComponentModel.DataAnnotations;

namespace CV_Manager.Domain.Models
{
    /// <summary>
    /// Represents professional experience information for a CV
    /// </summary>
    public class ExperienceInformation
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Company name is required")]
        [StringLength(20, ErrorMessage = "Company name cannot exceed 20 characters")]
        public string CompanyName { get; set; }

        [StringLength(50, ErrorMessage = "City name cannot exceed 50 characters")]
        public string City { get; set; }

        [StringLength(100, ErrorMessage = "Company field cannot exceed 100 characters")]
        public string CompanyField { get; set; }

        [Required(ErrorMessage = "CV ID is required")]
        public int CVId { get; set; }

        // Navigation property
        public virtual CV CV { get; set; }
    }
}