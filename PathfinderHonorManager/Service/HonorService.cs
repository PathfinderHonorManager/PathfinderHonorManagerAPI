using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using FluentValidation;
using System;
using System.Threading;
using System.Threading.Tasks;
using Incoming = PathfinderHonorManager.Dto.Incoming;
using Outgoing = PathfinderHonorManager.Dto.Outgoing;
using PathfinderHonorManager.Model;
using PathfinderHonorManager.DataAccess;
using PathfinderHonorManager.Service.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace PathfinderHonorManager.Service
{
    public class HonorService : IHonorService
    {
        private readonly PathfinderContext _dbContext;

        private readonly IMapper _mapper;

        private readonly ILogger _logger;

        private readonly IValidator<Incoming.PathfinderDto> _validator;


        public HonorService(
            PathfinderContext context,
            IMapper mapper,
            IValidator<Incoming.PathfinderDto> validator,
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

    }
}
