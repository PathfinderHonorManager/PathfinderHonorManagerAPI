using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PathfinderHonorManager.Service.Interfaces;
using Outgoing = PathfinderHonorManager.Dto.Outgoing;
using Incoming = PathfinderHonorManager.Dto.Incoming;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace PathfinderHonorManager.Controllers
{
    [ApiController]
    [Route("api/pathfinders")]
    [Authorize("ReadPathfinders")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public class PathfindersController : CustomApiController
    {
        private readonly IPathfinderService _pathfinderService;
        private readonly ILogger<PathfindersController> _logger;

        private string GetClubCodeFromContext()
        {
            var clubCode = HttpContext.User.FindFirst("clubCode")?.Value;
            return clubCode;
        }

        public PathfindersController(IPathfinderService pathfinderService, ILogger<PathfindersController> logger)
        {
            _pathfinderService = pathfinderService;
            _logger = logger;
        }

        // GET Pathfinders
        /// <summary>
        /// Get all Pathfinders
        /// </summary>
        /// <param name="token"></param>
        /// <param name="showInactive"></param>
        [HttpGet(Name = "GetAllPathfinders")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Outgoing.PathfinderDependantDto>>> GetAll(CancellationToken token, bool showInactive = false)
        {
            var clubCode = GetClubCodeFromContext();
            _logger.LogInformation("Getting all pathfinders for club {ClubCode}, showInactive: {ShowInactive}", clubCode, showInactive);
            
            var pathfinders = await _pathfinderService.GetAllAsync(clubCode, showInactive, token);

            if (pathfinders == null || !pathfinders.Any())
            {
                _logger.LogWarning("No pathfinders found for club {ClubCode}", clubCode);
                return NotFound();
            }

            _logger.LogInformation("Retrieved {Count} pathfinders for club {ClubCode}", pathfinders.Count, clubCode);
            return Ok(pathfinders);
        }

        // GET Pathfinders/{id}
        /// <summary>
        /// Get a Pathfinder by Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpGet("{id:guid}", Name = "GetPathfinderById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken token)
        {
            var clubCode = GetClubCodeFromContext();
            _logger.LogInformation("Getting pathfinder with ID {PathfinderId} for club {ClubCode}", id, clubCode);
            
            var pathfinder = await _pathfinderService.GetByIdAsync(id, clubCode, token);

            if (pathfinder == default)
            {
                _logger.LogWarning("Pathfinder with ID {PathfinderId} not found for club {ClubCode}", id, clubCode);
                return NotFound();
            }

            _logger.LogInformation("Retrieved pathfinder with ID {PathfinderId} for club {ClubCode}", id, clubCode);
            return Ok(pathfinder);
        }

        // POST Pathfinders
        /// <summary>
        /// Add a new Pathfinder
        /// </summary>
        /// <param name="newPathfinder"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("", Name = "CreatePathfinder")]
        [Authorize("CreatePathfinders")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        public async Task<IActionResult> PostAsync([FromBody] Incoming.PathfinderDto newPathfinder, CancellationToken token)
        {
            var clubCode = GetClubCodeFromContext();
            _logger.LogInformation("Creating new pathfinder for club {ClubCode}", clubCode);
            
            try
            {
                var pathfinder = await _pathfinderService.AddAsync(newPathfinder, clubCode, token);

                _logger.LogInformation("Created pathfinder with ID {PathfinderId} for club {ClubCode}", pathfinder.PathfinderID, clubCode);
                return CreatedAtRoute(
                    "GetPathfinderById",
                    new { id = pathfinder.PathfinderID },
                    pathfinder);
            }
            catch (FluentValidation.ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation failed while creating pathfinder for club {ClubCode}", clubCode);
                UpdateModelState(ex);
                return ValidationProblem(ModelState);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while creating pathfinder for club {ClubCode}", clubCode);
                return ValidationProblem(ex.Message);
            }
        }

        // PUT Pathfinders/{pathfinderId}
        /// <summary>
        /// Update a Pathfinder
        /// </summary>
        /// <param name="pathfinderId"></param>
        /// <param name="updatedPathfinder"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPut("{pathfinderId:guid}", Name = "UpdatePathfinder")]
        [Authorize("UpdatePathfinders")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PutAsync(Guid pathfinderId, [FromBody] Incoming.PutPathfinderDto updatedPathfinder, CancellationToken token)
        {
            var clubCode = GetClubCodeFromContext();
            _logger.LogInformation("Updating pathfinder with ID {PathfinderId} for club {ClubCode}", pathfinderId, clubCode);
            
            try
            {
                var pathfinder = await _pathfinderService.UpdateAsync(pathfinderId, updatedPathfinder, clubCode, token);

                if (pathfinder == default)
                {
                    _logger.LogWarning("Pathfinder with ID {PathfinderId} not found for club {ClubCode}", pathfinderId, clubCode);
                    return NotFound();
                }

                _logger.LogInformation("Updated pathfinder with ID {PathfinderId} for club {ClubCode}", pathfinderId, clubCode);
                return Ok(pathfinder);
            }
            catch (FluentValidation.ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation failed while updating pathfinder with ID {PathfinderId} for club {ClubCode}", pathfinderId, clubCode);
                UpdateModelState(ex);
                return ValidationProblem(ModelState);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while updating pathfinder with ID {PathfinderId} for club {ClubCode}", pathfinderId, clubCode);
                return ValidationProblem(ex.Message);
            }
        }

        // PUT Pathfinders/batch
        /// <summary>
        /// Update multiple Pathfinders at once
        /// </summary>
        /// <param name="bulkData"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPut("", Name = "BulkUpdatePathfinders")]
        [Authorize("UpdatePathfinders")]
        [ProducesResponseType(StatusCodes.Status207MultiStatus)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> BulkPutPathfindersAsync([FromBody] IEnumerable<Incoming.BulkPutPathfinderDto> bulkData, CancellationToken token)
        {
            var clubCode = GetClubCodeFromContext();
            _logger.LogInformation("Bulk updating {Count} pathfinders for club {ClubCode}", bulkData.Count(), clubCode);
            
            var responses = new List<object>();

            foreach (var data in bulkData)
            {
                foreach (var item in data.Items)
                {
                    responses.Add(await BuildBulkUpdateResponseAsync(item, clubCode, token));
                }
            }

            _logger.LogInformation("Completed bulk update of {Count} pathfinders for club {ClubCode}", bulkData.Count(), clubCode);
            return StatusCode(StatusCodes.Status207MultiStatus, responses);
        }

        private async Task<object> BuildBulkUpdateResponseAsync(Incoming.BulkPutPathfinderItemDto item, string clubCode, CancellationToken token)
        {
            try
            {
                var pathfinder = await _pathfinderService.UpdateAsync(
                    item.PathfinderId,
                    new Incoming.PutPathfinderDto { Grade = item.Grade, IsActive = item.IsActive },
                    clubCode,
                    token);

                if (pathfinder == null)
                {
                    _logger.LogWarning("Pathfinder with ID {PathfinderId} not found during bulk update for club {ClubCode}", item.PathfinderId, clubCode);
                    return new
                    {
                        status = StatusCodes.Status404NotFound,
                        pathfinderId = item.PathfinderId
                    };
                }

                _logger.LogInformation("Updated pathfinder with ID {PathfinderId} during bulk update for club {ClubCode}", item.PathfinderId, clubCode);
                return new
                {
                    status = StatusCodes.Status200OK,
                    pathfinderId = item.PathfinderId
                };
            }
            catch (FluentValidation.ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation failed while bulk updating pathfinder with ID {PathfinderId} for club {ClubCode}", item.PathfinderId, clubCode);
                return new
                {
                    status = StatusCodes.Status400BadRequest,
                    pathfinderId = item.PathfinderId,
                    errors = ex.Errors.Select(e => e.ErrorMessage)
                };
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while bulk updating pathfinder with ID {PathfinderId} for club {ClubCode}", item.PathfinderId, clubCode);
                return new
                {
                    status = StatusCodes.Status400BadRequest,
                    pathfinderId = item.PathfinderId,
                    error = ex.Message
                };
            }
        }
    }
}
