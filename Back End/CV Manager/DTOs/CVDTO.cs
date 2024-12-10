using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CV_Manager.DTOs
{
    public class CVDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public PersonalInformationDTO PersonalInformation { get; set; }
        public List<ExperienceInformationDTO> ExperienceInformation { get; set; }
    }

    /// <summary>
    /// DTO for creating new CVs
    /// </summary>
    public class CreateCVDTO
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Personal information is required")]
        public CreatePersonalInformationDTO PersonalInformation { get; set; }

        [Required(ErrorMessage = "At least one experience record is required")]
        public List<CreateExperienceInformationDTO> ExperienceInformation { get; set; }
    }

    /// <summary>
    /// DTO for updating existing CVs
    /// </summary>
    public class UpdateCVDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Personal information is required")]
        public PersonalInformationDTO PersonalInformation { get; set; }

        [Required(ErrorMessage = "Experience information is required")]
        public List<ExperienceInformationDTO> ExperienceInformation { get; set; }
    }
}
