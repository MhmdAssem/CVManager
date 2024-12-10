using CV_Manager.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CV_Manager.Domain.Interfaces
{
    public interface ICVRepository : IRepository<CV>
    {
        Task<IEnumerable<ExperienceInformation>> GetExperiencesAsync(int cvId);
        Task AddExperienceAsync(int cvId, ExperienceInformation experience);
        Task RemoveExperienceAsync(int cvId, int experienceId);
    }
}