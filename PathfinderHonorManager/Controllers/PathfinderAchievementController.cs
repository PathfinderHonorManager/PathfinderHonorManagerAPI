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
using System.Linq;

namespace PathfinderHonorManager.Controllers
{
    [ApiController]
    [Route("api/")]
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
        [Route("[controller]")]
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
        [Route("Pathfinders/{pathfinderId:guid}/[controller]/{achievementId:guid}")]
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
        [Route("Pathfinders/{pathfinderId:guid}/[controller]")]
        [Authorize("UpdatePathfinders")]
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

        // PUT api/Pathfinders/{pathfinderId}/PathfinderAchievements/{achievementId}
        [Route("Pathfinders/{pathfinderId:guid}/[controller]/{achievementId:guid}")]
        [Authorize("UpdatePathfinders")]
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Outgoing.PathfinderAchievementDto>> UpdatePathfinderAchievement(Guid pathfinderId, Guid achievementId, [FromBody] Incoming.PutPathfinderAchievementDto updatedAchievement, CancellationToken token)
        {
            try
            {
                var pathfinderAchievement = await _pathfinderAchievementService.UpdateAsync(pathfinderId, achievementId, updatedAchievement, token);
                if (pathfinderAchievement == null)
                {
                    return NotFound();
                }
                return Ok(pathfinderAchievement);
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

        // GET api/Pathfinders/{pathfinderId}/PathfinderAchievements
        [HttpGet]
        [Route("Pathfinders/{pathfinderId:guid}/PathfinderAchievements")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Outgoing.PathfinderAchievementDto>>> GetAllAchievementsForPathfinder(Guid pathfinderId, CancellationToken token)
        {
            var achievements = await _pathfinderAchievementService.GetAllAchievementsForPathfinderAsync(pathfinderId, token);

            if (achievements == null || !achievements.Any())
            {
                return NotFound();
            }

            return Ok(achievements);
        }

        // POST api/PathfinderAchievements
        [HttpPost]
        [Route("[controller]")]
        [Authorize("UpdatePathfinders")]
        [ProducesResponseType(StatusCodes.Status207MultiStatus)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddAchievementsForGrade([FromBody] Incoming.PostPathfinderAchievementForGradeDto dto, CancellationToken token)
        {
            var responses = new List<object>();

            foreach (var pathfinderId in dto.PathfinderIds)
            {
                try
                {
                    var achievements = await _pathfinderAchievementService.AddAchievementsForPathfinderAsync(pathfinderId, token);
                    responses.Add(new 
                    { 
                        pathfinderId = pathfinderId, 
                        achievements = achievements,
                        status = achievements.Any() ? StatusCodes.Status201Created : StatusCodes.Status404NotFound
                    });
                }
                catch (FluentValidation.ValidationException ex)
                {
                    responses.Add(new 
                    { 
                        pathfinderId = pathfinderId,
                        status = StatusCodes.Status400BadRequest,
                        error = ex.Errors.Select(e => e.ErrorMessage).ToList()
                    });
                }
            }

            return StatusCode(StatusCodes.Status207MultiStatus, responses);
        }
    }
}
