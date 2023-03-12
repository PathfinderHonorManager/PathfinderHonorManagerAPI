using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PathfinderHonorManager.Model;
using PathfinderHonorManager.Service.Interfaces;

namespace PathfinderHonorManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize("ReadClubs")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public class ClubsController : ControllerBase
    {
        private readonly IClubService _clubService;

        public ClubsController(IClubService clubService)
        {
            _clubService = clubService;
        }

        // GET Clubs
        /// <summary>  
        /// Get all Clubs or Clubs matching the given clubcode.
        /// </summary>
        /// <param name="clubcode">The club code of the club to retrieve</param>
        /// <param name="token"></param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Club>>> GetClubs(CancellationToken token, [FromQuery] string clubcode = null)
        {
            if (clubcode == null)
            {
                var clubs = await _clubService.GetAllAsync(token);

                if (clubs == default)
                {
                    return NotFound();
                }

                return Ok(clubs);
            }
            else
            {
                var club = await _clubService.GetByCodeAsync(clubcode.ToUpper(), token);

                if (club == default)
                {
                    return NotFound();
                }

                return Ok(club);
            }
        }

        // GET Clubs/{id}
        /// <summary>
        /// Get a Club by Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken token)
        {
            var club = await _clubService.GetByIdAsync(id, token);

            if (club == default)
            {
                return NotFound();
            }

            return Ok(club);
        }
    }
}
