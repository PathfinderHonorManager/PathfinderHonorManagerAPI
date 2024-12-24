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

        private string GetClubCodeFromContext()
        {
            var clubCode = HttpContext.User.FindFirst("clubCode")?.Value;
            return clubCode;
        }

        public PathfindersController(IPathfinderService pathfinderService)
        {
            _pathfinderService = pathfinderService;
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
            var pathfinders = await _pathfinderService.GetAllAsync(clubCode, showInactive, token);

            if (pathfinders == null || !pathfinders.Any())
            {
                return NotFound();
            }

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
            var pathfinder = await _pathfinderService.GetByIdAsync(id, clubCode, token);

            if (pathfinder == default)
            {
                return NotFound();
            }

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
            try
            {
                var pathfinder = await _pathfinderService.AddAsync(newPathfinder, clubCode, token);

                return CreatedAtRoute(
                    routeValues: GetByIdAsync(pathfinder.PathfinderID, token),
                    pathfinder);
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
            try
            {
                var pathfinder = await _pathfinderService.UpdateAsync(pathfinderId, updatedPathfinder, clubCode, token);

                return pathfinder != default
                    ? Ok(pathfinder)
                    : NotFound();
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
            var responses = new List<object>();

            foreach (var data in bulkData)
            {
                foreach (var item in data.Items)
                {
                    try
                    {
                        var pathfinder = await _pathfinderService.UpdateAsync(item.PathfinderId, new Incoming.PutPathfinderDto { Grade = item.Grade, IsActive = item.IsActive }, clubCode, token);

                        responses.Add(new
                        {
                            status = pathfinder != null ? StatusCodes.Status200OK : StatusCodes.Status404NotFound,
                            pathfinderId = item.PathfinderId,
                        });
                    }
                    catch (FluentValidation.ValidationException ex)
                    {
                        responses.Add(new
                        {
                            status = StatusCodes.Status400BadRequest,
                            pathfinderId = item.PathfinderId,
                            errors = ex.Errors.Select(e => e.ErrorMessage)
                        });
                    }
                    catch (DbUpdateException ex)
                    {
                        responses.Add(new
                        {
                            status = StatusCodes.Status400BadRequest,
                            pathfinderId = item.PathfinderId,
                            error = ex.Message
                        });
                    }
                }
            }

            return StatusCode(StatusCodes.Status207MultiStatus, responses);
        }
    }
}