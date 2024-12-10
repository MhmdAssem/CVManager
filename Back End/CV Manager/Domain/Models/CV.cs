using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CV_Manager.Domain.Models
{
    /// <summary>
    /// Represents the main CV entity that connects personal and experience information
    /// </summary>
    public class CV
    {
        public CV()
        {
            Experiences = new List<ExperienceInformation>();
        }

        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Personal Information ID is required")]
        public int Personal_Information_Id { get; set; }

        // Navigation properties
        public virtual PersonalInformation PersonalInformation { get; set; }
        public virtual ICollection<ExperienceInformation> Experiences { get; set; }
    }
}
