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

            // Validate the incoming honor DTO using the created validator
            _ = await _validator.ValidateAsync(newHonor,opt => opt.ThrowOnFailures(), token);

            // Map the validated honor DTO to a Honor entity
            var honor = _mapper.Map<Honor>(newHonor);

            // Add the honor to the database
            _dbContext.Honors.Add(honor);
            await _dbContext.SaveChangesAsync(token);

            // Map the added honor to an Outgoing Honor DTO and return it
            return _mapper.Map<Outgoing.HonorDto>(honor);
        }
    }
}
