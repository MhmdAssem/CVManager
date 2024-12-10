using AutoMapper;
using CV_Manager.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Transactions;
using System;
using CV_Manager.Domain.Interfaces;
using CV_Manager.Domain.Models;
using System.Linq;

namespace CV_Manager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CVController : ControllerBase
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IMapper _mapper;
        private readonly ILogger<CVController> _logger;

        public CVController(
            IServiceScopeFactory scopeFactory,
            IMapper mapper,
            ILogger<CVController> logger)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private async Task<T> ExecuteInScope<T>(Func<IServiceProvider, Task<T>> operation)
        {
            using var scope = _scopeFactory.CreateScope();
            return await operation(scope.ServiceProvider);
        }

        private async Task ExecuteInScope(Func<IServiceProvider, Task> operation)
        {
            using var scope = _scopeFactory.CreateScope();
            await operation(scope.ServiceProvider);
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CVDTO>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CVDTO>>> GetAllCVs()
        {
            try
            {
                _logger.LogInformation("Retrieving all CVs");

                var cvs = await ExecuteInScope(async provider =>
                {
                    var cvRepository = provider.GetRequiredService<ICVRepository>();
                    return await cvRepository.GetAllAsync();
                });

                var cvDtos = new List<CVDTO>();

                foreach (var cv in cvs)
                {
                    var personalInfo = await ExecuteInScope(async provider =>
                    {
                        var personalRepo = provider.GetRequiredService<IRepository<PersonalInformation>>();
                        return await personalRepo.GetByIdAsync(cv.Personal_Information_Id);
                    });

                    var experiences = await ExecuteInScope(async provider =>
                    {
                        var cvRepo = provider.GetRequiredService<ICVRepository>();
                        return await cvRepo.GetExperiencesAsync(cv.Id);
                    });

                    var cvDto = _mapper.Map<CVDTO>(cv);
                    cvDto.PersonalInformation = _mapper.Map<PersonalInformationDTO>(personalInfo);
                    cvDto.ExperienceInformation = _mapper.Map<List<ExperienceInformationDTO>>(experiences);
                    cvDtos.Add(cvDto);
                }

                return Ok(cvDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving CVs");
                return StatusCode(500, "An error occurred while retrieving CVs");
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CVDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CVDTO>> GetCV(int id)
        {
            try
            {
                _logger.LogInformation("Retrieving CV with ID: {CVId}", id);

                var cv = await ExecuteInScope(async provider =>
                {
                    var cvRepository = provider.GetRequiredService<ICVRepository>();
                    return await cvRepository.GetByIdAsync(id);
                });

                if (cv == null)
                {
                    _logger.LogWarning("CV not found with ID: {CVId}", id);
                    return NotFound($"CV with ID {id} not found");
                }

                var personalInfoTask = ExecuteInScope(async provider =>
                {
                    var personalRepo = provider.GetRequiredService<IRepository<PersonalInformation>>();
                    return await personalRepo.GetByIdAsync(cv.Personal_Information_Id);
                });

                var experiencesTask = ExecuteInScope(async provider =>
                {
                    var cvRepo = provider.GetRequiredService<ICVRepository>();
                    return await cvRepo.GetExperiencesAsync(id);
                });

                await Task.WhenAll(personalInfoTask, experiencesTask);

                var cvDto = _mapper.Map<CVDTO>(cv);
                cvDto.PersonalInformation = _mapper.Map<PersonalInformationDTO>(await personalInfoTask);
                cvDto.ExperienceInformation = _mapper.Map<List<ExperienceInformationDTO>>(await experiencesTask);

                return Ok(cvDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving CV with ID: {CVId}", id);
                return StatusCode(500, "An error occurred while retrieving the CV");
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(CVDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CVDTO>> CreateCV(CreateCVDTO createCvDTO)
        {
            try
            {
                _logger.LogInformation("Creating new CV");

                CV createdCV;
                PersonalInformation createdPersonalInfo;
                List<ExperienceInformation> experiences;

                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    // Create CV first
                    createdCV = await ExecuteInScope(async provider =>
                    {
                        var cvRepo = provider.GetRequiredService<ICVRepository>();
                        var cv = _mapper.Map<CV>(createCvDTO);
                        return await cvRepo.AddAsync(cv);
                    });

                    // Create experiences in parallel
                    experiences = new();
                    var experienceTasks = createCvDTO.ExperienceInformation.Select(async exp =>
                    {
                        await ExecuteInScope(async provider =>
                        {
                            var expRepo = provider.GetRequiredService<IRepository<ExperienceInformation>>();
                            var experience = _mapper.Map<ExperienceInformation>(exp);
                            experience.CVId = createdCV.Id;
                            var updatedExperience = await expRepo.AddAsync(experience);
                            experiences.Add(updatedExperience);
                        });
                    });

                    // Create personal information
                    createdPersonalInfo = await ExecuteInScope(async provider =>
                    {
                        var personalRepo = provider.GetRequiredService<IRepository<PersonalInformation>>();
                        var personalInfo = _mapper.Map<PersonalInformation>(createCvDTO.PersonalInformation);
                        return await personalRepo.AddAsync(personalInfo);
                    });

                    // Wait for all experiences to be created
                    await Task.WhenAll(experienceTasks);

                    // Update CV with personal info ID
                    createdCV.Personal_Information_Id = createdPersonalInfo.Id;
                    await ExecuteInScope(async provider =>
                    {
                        var cvRepo = provider.GetRequiredService<ICVRepository>();
                        await cvRepo.UpdateAsync(createdCV);
                    });

                    scope.Complete(); // Commit the transaction if all tasks succeed
                }

                // Prepare response
                var resultDto = _mapper.Map<CVDTO>(createdCV);
                resultDto.PersonalInformation = _mapper.Map<PersonalInformationDTO>(createdPersonalInfo);
                resultDto.ExperienceInformation = _mapper.Map<List<ExperienceInformationDTO>>(experiences);

                return CreatedAtAction(nameof(GetCV), new { id = resultDto.Id }, resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating CV");
                return StatusCode(500, "An error occurred while creating the CV");
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCV(int id, UpdateCVDTO updateCvDTO)
        {
            try
            {
                if (id != updateCvDTO.Id)
                    return BadRequest("ID mismatch");

                var existingCV = await ExecuteInScope(async provider =>
                {
                    var cvRepo = provider.GetRequiredService<ICVRepository>();
                    return await cvRepo.GetByIdAsync(id);
                });

                if (existingCV == null)
                    return NotFound($"CV with ID {id} not found");

                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    // Get current experiences first
                    var currentExperiences = await ExecuteInScope(async provider =>
                    {
                        var cvRepo = provider.GetRequiredService<ICVRepository>();
                        return await cvRepo.GetExperiencesAsync(id);
                    });

                    // Task 1: Handle Experiences
                    var experiencesTask = Task.Run(async () =>
                    {
                        var updatedExperienceIds = updateCvDTO.ExperienceInformation
                            .Where(e => e.Id != 0)
                            .Select(e => e.Id);

                        // Handle deletions
                        foreach (var exp in currentExperiences)
                        {
                            if (!updatedExperienceIds.Contains(exp.Id))
                            {
                                await ExecuteInScope(async provider =>
                                {
                                    var expRepo = provider.GetRequiredService<IRepository<ExperienceInformation>>();
                                    await expRepo.DeleteAsync(exp.Id);
                                });
                            }
                        }

                        // Handle updates and additions  
                        foreach (var expDto in updateCvDTO.ExperienceInformation)
                        {
                            await ExecuteInScope(async provider =>
                            {
                                var expRepo = provider.GetRequiredService<IRepository<ExperienceInformation>>();
                                var experience = _mapper.Map<ExperienceInformation>(expDto);
                                experience.CVId = id;

                                if (expDto.Id == 0)
                                {
                                    await expRepo.AddAsync(experience);
                                }
                                else
                                {
                                    await expRepo.UpdateAsync(experience);
                                }
                            });
                        }
                    });

                    await Task.WhenAll(experiencesTask);

                    // Task 2: Update Personal Information
                    var personalInfoTask = Task.Run(async () =>
                    {
                        await ExecuteInScope(async provider =>
                        {
                            var personalRepo = provider.GetRequiredService<IRepository<PersonalInformation>>();
                            var personalInfo = _mapper.Map<PersonalInformation>(updateCvDTO.PersonalInformation);
                            await personalRepo.UpdateAsync(personalInfo);
                        });
                    });
                    await Task.WhenAll(personalInfoTask);

                    // Task 3: Update CV
                    var cvUpdateTask = Task.Run(async () =>
                    {
                        await ExecuteInScope(async provider =>
                        {
                            var cvRepo = provider.GetRequiredService<ICVRepository>();
                            var cv = _mapper.Map<CV>(updateCvDTO);
                            await cvRepo.UpdateAsync(cv);
                        });
                    });
                    await Task.WhenAll(cvUpdateTask);

                    scope.Complete(); // Commit the transaction if all tasks succeed
                }

                return Ok(new { message = "Updated" });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating CV with ID: {CVId}", id);
                return StatusCode(500, "An error occurred while updating the CV");
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCV(int id)
        {
            try
            {
                var cv = await ExecuteInScope(async provider =>
                {
                    var cvRepo = provider.GetRequiredService<ICVRepository>();
                    return await cvRepo.GetByIdAsync(id);
                });

                if (cv == null)
                    return NotFound($"CV with ID {id} not found");

                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    // Task 1: Delete Experiences
                    var deleteExperiencesTask = Task.Run(async () =>
                    {
                        var experiences = await ExecuteInScope(async provider =>
                        {
                            var cvRepo = provider.GetRequiredService<ICVRepository>();
                            return await cvRepo.GetExperiencesAsync(id);
                        });

                        foreach (var exp in experiences)
                        {
                            await ExecuteInScope(async provider =>
                            {
                                var expRepo = provider.GetRequiredService<IRepository<ExperienceInformation>>();
                                await expRepo.DeleteAsync(exp.Id);
                            });
                        }
                    });

                    await Task.WhenAll(deleteExperiencesTask);

                    // Task 2: Delete CV
                    var deleteCVTask = Task.Run(async () =>
                    {
                        await ExecuteInScope(async provider =>
                        {
                            var cvRepo = provider.GetRequiredService<ICVRepository>();
                            await cvRepo.DeleteAsync(id);
                        });
                    });
                    await Task.WhenAll(deleteCVTask);

                    // Task 3: Delete Personal Information
                    var deletePersonalInfoTask = Task.Run(async () =>
                    {
                        await ExecuteInScope(async provider =>
                        {
                            var personalRepo = provider.GetRequiredService<IRepository<PersonalInformation>>();
                            await personalRepo.DeleteAsync(cv.Personal_Information_Id);
                        });
                    });

                    await Task.WhenAll(deletePersonalInfoTask);

                    scope.Complete(); // Commit the transaction if all tasks succeed
                }

                return Ok(new { message = "Deleted" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting CV with ID: {CVId}", id);
                return StatusCode(500, "An error occurred while deleting the CV");
            }
        }
    }
}