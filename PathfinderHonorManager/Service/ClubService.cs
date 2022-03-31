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
    public class ClubService : IClubService
    {
        private readonly PathfinderContext _dbContext;

        private readonly IMapper _mapper;

        private readonly ILogger _logger;

        private readonly IValidator<Incoming.PathfinderDto> _validator;


        public ClubService(
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

        public async Task<ICollection<Outgoing.ClubDto>> GetAllAsync(CancellationToken token)
        {
            var clubs = await _dbContext.Clubs
                .OrderBy(h => h.Name)
                .ToListAsync(token);

            return _mapper.Map<ICollection<Outgoing.ClubDto>>(clubs);

        }

        public async Task<Outgoing.ClubDto> GetByIdAsync(Guid id, CancellationToken token)
        {
            Club entity;

            entity = await _dbContext.Clubs
                .SingleOrDefaultAsync(p => p.ClubID == id, token);            

            return entity == default
                ? default
                : _mapper.Map<Outgoing.ClubDto>(entity);
        }

    }
}
