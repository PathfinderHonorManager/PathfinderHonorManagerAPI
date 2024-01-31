using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using PathfinderHonorManager.Model;
using PathfinderHonorManager.Service.Interfaces;
using Outgoing = PathfinderHonorManager.Dto.Outgoing;
using Incoming = PathfinderHonorManager.Dto.Incoming;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace PathfinderHonorManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize("ReadPathfinders")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public class PathfindersController : ApiController
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
        [HttpGet]
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
        [HttpGet("{id:guid}")]
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
        [HttpPost]
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

        // PUT Pathfinders
        /// <summary>
        /// Update a Pathfinder
        /// </summary>
        /// <param name="pathfinderId"></param>
        /// <param name="updatedPathfinder"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPut]
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
    }
}
