using CV_Manager.Domain.Interfaces;
using CV_Manager.Domain.Models;
using CV_Manager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CV_Manager.Infrastructure.Repositories
{
    public class CVRepository : Repository<CV>, ICVRepository
    {
        private readonly ILogger<CVRepository> _logger;

        public CVRepository(
            CVManagerContext context,
            ILogger<CVRepository> logger) : base(context, logger)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<ExperienceInformation>> GetExperiencesAsync(int cvId)
        {
            try
            {
                // Now we can directly query experiences using the CVId foreign key
                var experiences = await _context.ExperienceInformation
                    .Where(e => e.CVId == cvId)
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} experiences for CV ID: {CVId}",
                    experiences.Count, cvId);

                return experiences;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving experiences for CV ID: {CVId}", cvId);
                throw;
            }
        }

        public async Task AddExperienceAsync(int cvId, ExperienceInformation experience)
        {
            try
            {
                // Set the CVId before adding the experience
                experience.CVId = cvId;

                // Add the experience directly - no need for a junction table anymore
                await _context.ExperienceInformation.AddAsync(experience);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Added experience with ID {ExperienceId} to CV {CVId}",
                    experience.Id, cvId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error adding experience to CV {CVId}", cvId);
                throw;
            }
        }

        public async Task RemoveExperienceAsync(int cvId, int experienceId)
        {
            try
            {
                // Find the experience that belongs to this CV
                var experience = await _context.ExperienceInformation
                    .FirstOrDefaultAsync(e => e.CVId == cvId && e.Id == experienceId);

                if (experience != null)
                {
                    _context.ExperienceInformation.Remove(experience);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation(
                        "Removed experience {ExperienceId} from CV {CVId}",
                        experienceId, cvId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error removing experience {ExperienceId} from CV {CVId}",
                    experienceId, cvId);
                throw;
            }
        }

        public async Task<bool> HasExperienceAsync(int cvId, int experienceId)
        {
            try
            {
                // Check if the experience exists and belongs to this CV
                return await _context.ExperienceInformation
                    .AnyAsync(e => e.CVId == cvId && e.Id == experienceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error checking experience {ExperienceId} for CV {CVId}",
                    experienceId, cvId);
                throw;
            }
        }

        // Override the base delete method to ensure experiences are properly handled
        public override async Task DeleteAsync(int id)
        {
            try
            {
                // First, delete all experiences associated with this CV
                var experiences = await _context.ExperienceInformation
                    .Where(e => e.CVId == id)
                    .ToListAsync();

                if (experiences.Any())
                {
                    _context.ExperienceInformation.RemoveRange(experiences);
                    await _context.SaveChangesAsync();
                }

                // Then delete the CV itself using the base class method
                await base.DeleteAsync(id);

                _logger.LogInformation("Deleted CV {CVId} and all its experiences", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting CV {CVId}", id);
                throw;
            }
        }
    }
}