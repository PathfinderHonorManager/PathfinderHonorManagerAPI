using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
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

        private readonly ILogger _logger;

        private readonly IValidator<Incoming.HonorDto> _validator;


        public HonorService(
            PathfinderContext context,
            IMapper mapper,
            IValidator<Incoming.HonorDto> validator,
            ILogger<PathfinderService> logger)
        {
            _dbContext = context;
            _mapper = mapper;
            _logger = logger;
            _validator = validator;
        }

        public async Task<ICollection<Outgoing.HonorDto>> GetAllAsync(CancellationToken token)
        {
            var honors = await _dbContext.Honors
                .OrderBy(h => h.Name).ThenBy(h => h.Level)
                .ToListAsync(token);

            return _mapper.Map<ICollection<Outgoing.HonorDto>>(honors);

        }

        public async Task<Outgoing.HonorDto> GetByIdAsync(Guid id, CancellationToken token)
        {
            Honor entity;

            entity = await _dbContext.Honors
                .SingleOrDefaultAsync(p => p.HonorID == id, token);            

            return entity == default
                ? default
                : _mapper.Map<Outgoing.HonorDto>(entity);
        }

        public async Task<Outgoing.HonorDto> AddAsync(Incoming.HonorDto newHonor, CancellationToken token)
        {

            _ = await _validator.ValidateAsync(
                newHonor,
                opt => opt
                    .ThrowOnFailures()
                    .IncludeAllRuleSets(),
                token);

            var honor = _mapper.Map<Honor>(newHonor);

            _dbContext.Honors.Add(honor);
            await _dbContext.SaveChangesAsync(token);

            return _mapper.Map<Outgoing.HonorDto>(honor);
        }

        public async Task<Outgoing.HonorDto> UpdateAsync(Guid id, Incoming.HonorDto updatedHonor, CancellationToken token)
        {
            _ = await _validator.ValidateAsync(updatedHonor, opt => opt.ThrowOnFailures(), token);

            var existingHonor = await GetByIdAsync(id, token);

            if (existingHonor == null)
            {
                return null;
            }

            existingHonor.Name = updatedHonor.Name;
            existingHonor.Level = updatedHonor.Level;
            existingHonor.PatchFilename = updatedHonor.PatchFilename;
            existingHonor.WikiPath = updatedHonor.WikiPath;

            await _dbContext.SaveChangesAsync(token);

            return _mapper.Map<Outgoing.HonorDto>(existingHonor);
        }

    }
}
