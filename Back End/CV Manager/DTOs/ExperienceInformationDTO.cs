﻿using System.ComponentModel.DataAnnotations;

namespace CV_Manager.DTOs
{
    /// <summary>
    /// Data Transfer Object for professional experience information
    /// </summary>
    public class ExperienceInformationDTO
    {
        /// <summary>
        /// Unique identifier for the experience information record
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of the company
        /// Required field, maximum 20 characters as per specification
        /// </summary>
        [Required(ErrorMessage = "Company name is required")]
        [StringLength(20, ErrorMessage = "Company name cannot exceed 20 characters")]
        public string CompanyName { get; set; }

        /// <summary>
        /// City where the company is located
        /// Optional field
        /// </summary>
        [StringLength(50, ErrorMessage = "City name cannot exceed 50 characters")]
        public string City { get; set; }

        /// <summary>
        /// Field or industry of the company
        /// Optional field
        /// </summary>
        [StringLength(100, ErrorMessage = "Company field cannot exceed 100 characters")]
        public string CompanyField { get; set; }
    }

    /// <summary>
    /// DTO for creating new experience information records
    /// Excludes the Id field as it will be generated by the database
    /// </summary>
    public class CreateExperienceInformationDTO
    {
        [Required(ErrorMessage = "Company name is required")]
        [StringLength(20, ErrorMessage = "Company name cannot exceed 20 characters")]
        public string CompanyName { get; set; }

        [StringLength(50, ErrorMessage = "City name cannot exceed 50 characters")]
        public string City { get; set; }

        [StringLength(100, ErrorMessage = "Company field cannot exceed 100 characters")]
        public string CompanyField { get; set; }
    }
}