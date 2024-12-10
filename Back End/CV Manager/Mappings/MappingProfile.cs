using AutoMapper;
using CV_Manager.Domain.Models;
using CV_Manager.DTOs;

namespace CV_Manager.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // CV mappings
            CreateMap<CV, CVDTO>();
            CreateMap<CreateCVDTO, CV>();
            CreateMap<UpdateCVDTO, CV>();

            // Personal Information mappings
            CreateMap<PersonalInformation, PersonalInformationDTO>();
            CreateMap<CreatePersonalInformationDTO, PersonalInformation>();
            CreateMap<PersonalInformationDTO, PersonalInformation>();

            // Experience Information mappings
            CreateMap<ExperienceInformation, ExperienceInformationDTO>();
            CreateMap<CreateExperienceInformationDTO, ExperienceInformation>();
            CreateMap<ExperienceInformationDTO, ExperienceInformation>();
        }
    }
}