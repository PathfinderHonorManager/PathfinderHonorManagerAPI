using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PathfinderHonorManager.Dto.Outgoing;
using PathfinderHonorManager.Service.Interfaces;

namespace PathfinderHonorManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize("ReadHonors")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public class PathfinderAchievementsController : ControllerBase
    {
        private readonly IPathfinderAchievementService _pathfinderAchievementService;

        public PathfinderAchievementsController(IPathfinderAchievementService pathfinderAchievementService)
        {
            _pathfinderAchievementService = pathfinderAchievementService;
        }

        // GET api/PathfinderAchievement
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<PathfinderAchievementDto>>> GetPathfinderAchievements(CancellationToken token)
        {
            var pathfinderAchievements = await _pathfinderAchievementService.GetAllAsync(token);

            if (pathfinderAchievements == null || pathfinderAchievements.Count == 0)
            {
                return NotFound();
            }

            return Ok(pathfinderAchievements);
        }

        // GET api/PathfinderAchievement/{id}
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PathfinderAchievementDto>> GetPathfinderAchievementById(Guid id, CancellationToken token)
        {
            var pathfinderAchievement = await _pathfinderAchievementService.GetByIdAsync(id, token);

            if (pathfinderAchievement == null)
            {
                return NotFound();
            }

            return Ok(pathfinderAchievement);
        }
    }
}
