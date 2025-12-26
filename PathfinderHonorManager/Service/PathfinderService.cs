using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
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
    public class PathfinderService : IPathfinderService
    {
        private readonly PathfinderContext _dbContext;

        private readonly IClubService _clubService;

        private readonly IMapper _mapper;

        private readonly ILogger<PathfinderService> _logger;

        private readonly IValidator<Incoming.PathfinderDtoInternal> _validator;

        private readonly IGradeChangeQueue _gradeChangeQueue;

        private IQueryable<Outgoing.PathfinderDependantDto> QueryPathfindersWithAchievementCountsAsync(string clubCode, bool showInactive)
        {
            var query = _dbContext.Pathfinders
                .Where(c => c.Club.ClubCode == clubCode)
                .ProjectTo<Outgoing.PathfinderDependantDto>(_mapper.ConfigurationProvider);

            if (!showInactive)
            {
                query = query.Where(p => (bool)p.IsActive);
            }

            return query;
        }

        private IQueryable<Pathfinder> QueryPathfinderByIdAsync(Guid pathfinderId)
        {
            return _dbContext.Pathfinders
                .Include(pc => pc.PathfinderClass)
                .Include(c => c.Club)
                .Where(p => p.PathfinderID == pathfinderId);
        }

        public PathfinderService(
            PathfinderContext context,
            IClubService clubService,
            IMapper mapper,
            IValidator<Incoming.PathfinderDtoInternal> validator,
            ILogger<PathfinderService> logger,
            IGradeChangeQueue gradeChangeQueue)
        {
            _dbContext = context;
            _clubService = clubService;
            _mapper = mapper;
            _logger = logger;
            _validator = validator;
            _gradeChangeQueue = gradeChangeQueue;
        }

        public async Task<ICollection<Outgoing.PathfinderDependantDto>> GetAllAsync(string clubCode, bool showInactive, CancellationToken token)
        {
            _logger.LogInformation("Getting all pathfinders for club {ClubCode}, showInactive: {ShowInactive}", clubCode, showInactive);
            
            ICollection<Outgoing.PathfinderDependantDto> pathfinderDtos = await QueryPathfindersWithAchievementCountsAsync(clubCode, showInactive)
                .OrderBy(p => p.Grade)
                .ThenBy(p => p.LastName)
                .ToListAsync(token);

            _logger.LogInformation("Retrieved {Count} pathfinders for club {ClubCode}", pathfinderDtos.Count, clubCode);
            return pathfinderDtos;
        }

        public async Task<Outgoing.PathfinderDependantDto> GetByIdAsync(Guid id, string clubCode, CancellationToken token)
        {
            _logger.LogInformation("Getting pathfinder with ID {PathfinderId} for club {ClubCode}", id, clubCode);
            
            Outgoing.PathfinderDependantDto dto = await QueryPathfindersWithAchievementCountsAsync(clubCode, true)
                    .SingleOrDefaultAsync(p => p.PathfinderID == id, token);

            if (dto == default)
            {
                _logger.LogWarning("Pathfinder with ID {PathfinderId} not found for club {ClubCode}", id, clubCode);
            }
            else
            {
                _logger.LogInformation("Retrieved pathfinder with ID {PathfinderId} for club {ClubCode}", id, clubCode);
            }

            return dto;
        }

        public async Task<Outgoing.PathfinderDto> AddAsync(Incoming.PathfinderDto newPathfinder, string clubCode, CancellationToken token)
        {
            _logger.LogInformation("Adding new pathfinder for club {ClubCode}", clubCode);
            
            var club = await _clubService.GetByCodeAsync(clubCode, token);
            if (club == null)
            {
                _logger.LogWarning("Club with code {ClubCode} not found while adding pathfinder", clubCode);
            }

            var newPathfinderWithClubId = new Incoming.PathfinderDtoInternal()
            {
                FirstName = newPathfinder.FirstName,
                LastName = newPathfinder.LastName,
                Email = newPathfinder.Email,
                Grade = newPathfinder.Grade,
                ClubID = club?.ClubID ?? Guid.Empty
            };

            try
            {
                await _validator.ValidateAsync(
                    newPathfinderWithClubId,
                    opts => opts.ThrowOnFailures()
                            .IncludeAllRuleSets(),
                    token);

                var newEntity = _mapper.Map<Pathfinder>(newPathfinderWithClubId);

                await _dbContext.AddAsync(newEntity, token);
                await _dbContext.SaveChangesAsync(token);
                _logger.LogInformation("Added pathfinder with ID {PathfinderId} to database for club {ClubCode}", newEntity.PathfinderID, clubCode);

                var createdPathfinder = await GetByIdAsync(newEntity.PathfinderID, clubCode, token);

                return _mapper.Map<Outgoing.PathfinderDto>(createdPathfinder);
            }
            catch (FluentValidation.ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding pathfinder for club {ClubCode}", clubCode);
                throw new InvalidOperationException($"Failed to add pathfinder for club {clubCode}", ex);
            }
        }

        public async Task<Outgoing.PathfinderDto> UpdateAsync(Guid pathfinderId, Incoming.PutPathfinderDto updatedPathfinder, string clubCode, CancellationToken token)
        {
            _logger.LogInformation("Updating pathfinder with ID {PathfinderId} for club {ClubCode}", pathfinderId, clubCode);
            
            Pathfinder targetPathfinder = await QueryPathfinderByIdAsync(pathfinderId)
                                        .SingleOrDefaultAsync(token);

            if (targetPathfinder == default)
            {
                _logger.LogWarning("Pathfinder with ID {PathfinderId} not found", pathfinderId);
                return default;
            }

            var currentClub = await _clubService.GetByCodeAsync(clubCode, token);
            if (currentClub == null)
            {
                _logger.LogWarning("Current club with code {ClubCode} not found while updating pathfinder {PathfinderId}", clubCode, pathfinderId);
                return default;
            }

            try
            {
                var oldGrade = targetPathfinder.Grade;

                Incoming.PathfinderDtoInternal mappedPathfinder = new()
                {
                    FirstName = targetPathfinder.FirstName,
                    LastName = targetPathfinder.LastName,
                    Email = targetPathfinder.Email,
                    Grade = updatedPathfinder.Grade,
                    IsActive = updatedPathfinder.IsActive,
                    ClubID = updatedPathfinder.ClubID ?? targetPathfinder.ClubID
                };

                await _validator.ValidateAsync(
                    mappedPathfinder,
                    opts => opts.ThrowOnFailures()
                              .IncludeRuleSets("update"),
                    token);

                bool gradeChanged = false;
                if (mappedPathfinder.Grade != null)
                {
                    if (targetPathfinder.Grade != mappedPathfinder.Grade)
                    {
                        gradeChanged = true;
                    }
                    targetPathfinder.Grade = mappedPathfinder.Grade;
                }
                else
                {
                    if (targetPathfinder.Grade != null)
                    {
                        gradeChanged = true;
                    }
                    targetPathfinder.Grade = null;
                }
                
                if (mappedPathfinder.IsActive.HasValue)
                {
                    targetPathfinder.IsActive = mappedPathfinder.IsActive;
                }

                if (updatedPathfinder.ClubID.HasValue)
                {
                    targetPathfinder.ClubID = updatedPathfinder.ClubID.Value;
                }

                await _dbContext.SaveChangesAsync(token);
                _logger.LogInformation("Updated pathfinder with ID {PathfinderId} for club {ClubCode}", pathfinderId, clubCode);

                if (gradeChanged)
                {
                    var gradeChangeEvent = new GradeChangeEvent(pathfinderId, oldGrade, targetPathfinder.Grade);
                    await _gradeChangeQueue.TryEnqueueAsync(gradeChangeEvent, token);
                    _logger.LogInformation("Grade change detected for pathfinder {PathfinderId}: {OldGrade} → {NewGrade}, queued for achievement sync", pathfinderId, oldGrade, targetPathfinder.Grade);
                }

                return _mapper.Map<Outgoing.PathfinderDto>(targetPathfinder);
            }
            catch (FluentValidation.ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating pathfinder with ID {PathfinderId} for club {ClubCode}", pathfinderId, clubCode);
                throw new InvalidOperationException($"Failed to update pathfinder {pathfinderId} for club {clubCode}", ex);
            }
        }

        public async Task<ICollection<Outgoing.PathfinderDto>> BulkUpdateAsync(
            IEnumerable<Incoming.BulkPutPathfinderDto> bulkData,
            string clubCode,
            CancellationToken token)
        {
            _logger.LogInformation("Bulk updating {Count} pathfinders for club {ClubCode}", bulkData.Count(), clubCode);
            
            var updatedPathfinders = new List<Outgoing.PathfinderDto>();

            foreach (var data in bulkData)
            {
                foreach (var item in data.Items)
                {
                    try
                    {
                        var targetPathfinder = await QueryPathfinderByIdAsync(item.PathfinderId)
                                            .SingleOrDefaultAsync(token);

                        if (targetPathfinder != null)
                        {
                            var oldGrade = targetPathfinder.Grade;
                            bool gradeChanged = false;

                            if (item.Grade.HasValue)
                            {
                                if (targetPathfinder.Grade != item.Grade.Value)
                                {
                                    gradeChanged = true;
                                }
                                targetPathfinder.Grade = item.Grade.Value;
                            }

                            if (item.IsActive.HasValue)
                            {
                                targetPathfinder.IsActive = item.IsActive.Value;
                            }

                            var mappedPathfinder = _mapper.Map<Incoming.PathfinderDtoInternal>(targetPathfinder);
                            await _validator.ValidateAsync(mappedPathfinder, opts => opts.ThrowOnFailures(), token);

                            if (gradeChanged)
                            {
                                var gradeChangeEvent = new GradeChangeEvent(item.PathfinderId, oldGrade, targetPathfinder.Grade);
                                await _gradeChangeQueue.TryEnqueueAsync(gradeChangeEvent, token);
                                _logger.LogInformation("Grade change detected for pathfinder {PathfinderId}: {OldGrade} → {NewGrade}, queued for achievement sync", item.PathfinderId, oldGrade, targetPathfinder.Grade);
                            }

                            updatedPathfinders.Add(_mapper.Map<Outgoing.PathfinderDto>(targetPathfinder));
                            _logger.LogInformation("Updated pathfinder with ID {PathfinderId} during bulk update for club {ClubCode}", item.PathfinderId, clubCode);
                        }
                        else
                        {
                            _logger.LogWarning("Pathfinder with ID {PathfinderId} not found during bulk update for club {ClubCode}", item.PathfinderId, clubCode);
                        }
                    }
                    catch (FluentValidation.ValidationException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error updating pathfinder with ID {PathfinderId} during bulk update for club {ClubCode}", item.PathfinderId, clubCode);
                        throw new InvalidOperationException($"Failed to update pathfinder {item.PathfinderId} during bulk update for club {clubCode}", ex);
                    }
                }
            }

            await _dbContext.SaveChangesAsync(token);
            _logger.LogInformation("Completed bulk update of {Count} pathfinders for club {ClubCode}", bulkData.Count(), clubCode);

            return updatedPathfinders;
        }
    }
}