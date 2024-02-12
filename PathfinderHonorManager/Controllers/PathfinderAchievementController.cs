using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Outgoing = PathfinderHonorManager.Dto.Outgoing;
using Incoming = PathfinderHonorManager.Dto.Incoming;
using PathfinderHonorManager.Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace PathfinderHonorManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize("ReadHonors")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public class PathfinderAchievementsController : ApiController
    {
        private readonly IPathfinderAchievementService _pathfinderAchievementService;

        public PathfinderAchievementsController(IPathfinderAchievementService pathfinderAchievementService)
        {
            _pathfinderAchievementService = pathfinderAchievementService;
        }

        // GET api/PathfinderAchievements
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Outgoing.PathfinderAchievementDto>>> GetPathfinderAchievements(CancellationToken token)
        {
            var pathfinderAchievements = await _pathfinderAchievementService.GetAllAsync(token);

            if (pathfinderAchievements == null || pathfinderAchievements.Count == 0)
            {
                return NotFound();
            }

            return Ok(pathfinderAchievements);
        }

        // GET api/Pathfinders/{pathfinderId}/PathfinderAchievements/{achievementId}
        [Route("{pathfinderId:guid}/[controller]/{achievementId:guid}")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Outgoing.PathfinderAchievementDto>> GetPathfinderAchievementById(Guid pathfinderId, Guid achievementId, CancellationToken token)
        {
            var pathfinderAchievement = await _pathfinderAchievementService.GetByIdAsync(pathfinderId, achievementId, token);

            if (pathfinderAchievement == null)
            {
                return NotFound();
            }

            return Ok(pathfinderAchievement);
        }

        // POST api/Pathfinders/{pathfinderId}/PathfinderAchievements
        [Route("{pathfinderId:guid}/[controller]")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Outgoing.PathfinderAchievementDto>> PostAsync(Guid pathfinderId, [FromBody] Incoming.PostPathfinderAchievementDto pathfinderAchievementDto, CancellationToken token)
        {
            try
            {
                var pathfinderAchievement = await _pathfinderAchievementService.AddAsync(pathfinderId, pathfinderAchievementDto, token);
                return CreatedAtAction(nameof(GetPathfinderAchievementById), new { pathfinderId = pathfinderId, achievementId = pathfinderAchievement.AchievementID }, pathfinderAchievement);
            }
            catch (FluentValidation.ValidationException ex)
            {
                UpdateModelState(ex);
                return ValidationProblem(ModelState);
            }
            catch (DbUpdateException ex)
            {
                return ValidationProblem(ex.Message);
            }
        }
    }
}
