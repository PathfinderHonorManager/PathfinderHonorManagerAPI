using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PathfinderHonorManager.DataAccess;
using PathfinderHonorManager.Model;
using PathfinderHonorManager.Service.Interfaces;
using Incoming = PathfinderHonorManager.Dto.Incoming;
using Outgoing = PathfinderHonorManager.Dto.Outgoing;

namespace PathfinderHonorManager.Service
{
    public class HonorService : IHonorService
    {
        private readonly PathfinderContext _dbContext;

        private readonly IMapper _mapper;

        private readonly ILogger<HonorService> _logger;

        private readonly IValidator<Incoming.HonorDto> _validator;


        public HonorService(
            PathfinderContext context,
            IMapper mapper,
            IValidator<Incoming.HonorDto> validator,
            ILogger<HonorService> logger)
        {
            _dbContext = context;
            _mapper = mapper;
            _logger = logger;
            _validator = validator;
        }

        public async Task<ICollection<Outgoing.HonorDto>> GetAllAsync(CancellationToken token)
        {
            _logger.LogInformation("Getting all honors");
            var honors = await _dbContext.Honors
                .OrderBy(h => h.Name).ThenBy(h => h.Level)
                .ToListAsync(token);

            _logger.LogInformation("Retrieved {Count} honors", honors.Count);
            return _mapper.Map<ICollection<Outgoing.HonorDto>>(honors);
        }

        public async Task<Outgoing.HonorDto> GetByIdAsync(Guid id, CancellationToken token)
        {
            _logger.LogInformation("Getting honor with ID {HonorId}", id);
            Honor entity = await _dbContext.Honors
                .SingleOrDefaultAsync(p => p.HonorID == id, token);

            if (entity == default)
            {
                _logger.LogWarning("Honor with ID {HonorId} not found", id);
            }
            else
            {
                _logger.LogInformation("Retrieved honor with ID {HonorId}", id);
            }

            return entity == default
                ? default
                : _mapper.Map<Outgoing.HonorDto>(entity);
        }

        public async Task<Outgoing.HonorDto> AddAsync(Incoming.HonorDto newHonor, CancellationToken token)
        {
            _logger.LogInformation("Adding new honor");
            try
            {
                await _validator.ValidateAsync(
                    newHonor,
                    opt => opt
                        .ThrowOnFailures()
                        .IncludeAllRuleSets(),
                    token);

                var honor = _mapper.Map<Honor>(newHonor);

                _dbContext.Honors.Add(honor);
                await _dbContext.SaveChangesAsync(token);
                _logger.LogInformation("Added honor with ID {HonorId}", honor.HonorID);

                return _mapper.Map<Outgoing.HonorDto>(honor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding honor");
                throw;
            }
        }

        public async Task<Outgoing.HonorDto> UpdateAsync(Guid id, Incoming.HonorDto updatedHonor, CancellationToken token)
        {
            _logger.LogInformation("Updating honor with ID {HonorId}", id);
            try
            {
                await _validator.ValidateAsync(updatedHonor, opt => opt.ThrowOnFailures(), token);

                var existingHonor = await GetByIdAsync(id, token);

                if (existingHonor == null)
                {
                    _logger.LogWarning("Honor with ID {HonorId} not found", id);
                    return null;
                }

                existingHonor.Name = updatedHonor.Name;
                existingHonor.Level = updatedHonor.Level;
                existingHonor.PatchFilename = updatedHonor.PatchFilename;
                existingHonor.WikiPath = updatedHonor.WikiPath;

                await _dbContext.SaveChangesAsync(token);
                _logger.LogInformation("Updated honor with ID {HonorId}", id);

                return _mapper.Map<Outgoing.HonorDto>(existingHonor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating honor with ID {HonorId}", id);
                throw;
            }
        }

    }
}
