using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PathfinderHonorManager.Model;
using PathfinderHonorManager.Service.Interfaces;
using Microsoft.Extensions.Logging;
using System.Linq;
using PathfinderHonorManager.Dto.Incoming;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace PathfinderHonorManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize("ReadClubs")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public class ClubsController : CustomApiController
    {
        private readonly IClubService _clubService;
        private readonly ILogger<ClubsController> _logger;

        public ClubsController(
            IClubService clubService, 
            ILogger<ClubsController> logger,
            IValidator<ClubDto> validator)
        {
            _clubService = clubService;
            _logger = logger;
            _validator = validator;
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
                _logger.LogInformation("Getting all clubs");
                var clubs = await _clubService.GetAllAsync(token);

                if (clubs == default)
                {
                    _logger.LogWarning("No clubs found");
                    return NotFound();
                }

                _logger.LogInformation("Retrieved {Count} clubs", clubs.Count());
                return Ok(clubs);
            }
            else
            {
                _logger.LogInformation("Getting club with code {ClubCode}", clubcode);
                var club = await _clubService.GetByCodeAsync(clubcode.ToUpper(), token);

                if (club == default)
                {
                    _logger.LogWarning("Club with code {ClubCode} not found", clubcode);
                    return NotFound();
                }

                _logger.LogInformation("Retrieved club with code {ClubCode}", clubcode);
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
        [HttpGet("{id:guid}", Name = "GetClubById")]
        public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken token)
        {
            _logger.LogInformation("Getting club with ID {ClubId}", id);
            var club = await _clubService.GetByIdAsync(id, token);

            if (club == default)
            {
                _logger.LogWarning("Club with ID {ClubId} not found", id);
                return NotFound();
            }

            _logger.LogInformation("Retrieved club with ID {ClubId}", id);
            return Ok(club);
        }

        [Authorize("CreateClubs")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] ClubDto club, CancellationToken token)
        {
            _logger.LogInformation("Creating new club with code {ClubCode}", club.ClubCode);

            try
            {
                var createdClub = await _clubService.CreateAsync(club, token);
                return CreatedAtRoute("GetClubById", new { id = createdClub.ClubID }, createdClub);
            }
            catch (FluentValidation.ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation failed while creating club with code {ClubCode}", club.ClubCode);
                UpdateModelState(ex);
                return ValidationProblem(ModelState);
            }
        }

        [Authorize("UpdateClubs")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] ClubDto club, CancellationToken token)
        {
            _logger.LogInformation("Updating club with ID {ClubId}", id);

            try
            {
                var updatedClub = await _clubService.UpdateAsync(id, club, token);
                if (updatedClub == default)
                {
                    _logger.LogWarning("Club with ID {ClubId} not found for update", id);
                    return NotFound();
                }

                return Ok(updatedClub);
            }
            catch (FluentValidation.ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation failed while updating club with ID {ClubId}", id);
                UpdateModelState(ex);
                return ValidationProblem(ModelState);
            }
        }
    }
}
