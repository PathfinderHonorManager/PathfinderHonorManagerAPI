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
    public class AchievementsController : ControllerBase
    {
        private readonly IAchievementService _achievementService;

        public AchievementsController(IAchievementService achievementService)
        {
            _achievementService = achievementService;
        }

        // GET api/Achievement
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<AchievementDto>>> GetAchievements(CancellationToken token)
        {
            var achievements = await _achievementService.GetAllAsync(token);

            if (achievements == null || achievements.Count == 0)
            {
                return NotFound();
            }

            return Ok(achievements);
        }

        // GET api/Achievement/{id}
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AchievementDto>> GetAchievementById(Guid id, CancellationToken token)
        {
            var achievement = await _achievementService.GetByIdAsync(id, token);

            if (achievement == null)
            {
                return NotFound();
            }

            return Ok(achievement);
        }
    }
}
