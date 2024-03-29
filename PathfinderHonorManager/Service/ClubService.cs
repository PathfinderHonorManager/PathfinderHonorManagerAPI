﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
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

        public ClubService(
            PathfinderContext context,
            IMapper mapper,
            ILogger<ClubService> logger)
        {
            _dbContext = context;
            _mapper = mapper;
            _logger = logger;
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

        public async Task<Outgoing.ClubDto> GetByCodeAsync(string code, CancellationToken token)
        {
            code = code.ToUpper();

            Club entity;

            entity = await _dbContext.Clubs
                .SingleOrDefaultAsync(p => p.ClubCode == code, token);

            return entity == default
                ? default
                : _mapper.Map<Outgoing.ClubDto>(entity);
        }
    }
}
