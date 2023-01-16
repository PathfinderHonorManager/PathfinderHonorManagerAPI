using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PathfinderHonorManager.Model;
using PathfinderHonorManager.Service.Interfaces;
using Outgoing = PathfinderHonorManager.Dto.Outgoing;
using Incoming = PathfinderHonorManager.Dto.Incoming;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

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

        public PathfindersController(IPathfinderService pathfinderService)
        {
            _pathfinderService = pathfinderService;
        }

        // GET Pathfinders
        /// <summary>
        /// Get all Pathfinders
        /// </summary>
        /// <param name="token"></param>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Outgoing.PathfinderDependantDto>>> GetAll(CancellationToken token)
        {
            var pathfinder = await _pathfinderService.GetAllAsync(token);

            if (pathfinder == default)
            {
                return NotFound();
            }

            return Ok(pathfinder);
        }

        // GET Pathfinders/{id}
        /// <summary>
        /// Get a Pathfinder by Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken token)
        {
            var pathfinder = await _pathfinderService.GetByIdAsync(id, token);

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
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
        public async Task<IActionResult> PostAsync([FromBody] Incoming.PathfinderDto newPathfinder, CancellationToken token)
        {
            try
            {
                var pathfinder = await _pathfinderService.AddAsync(newPathfinder, token);

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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PutAsync(Guid pathfinderId, [FromBody] Incoming.PutPathfinderDto updatedPathfinder, CancellationToken token)
        {
            try
            {
                var pathfinder = await _pathfinderService.UpdateAsync(pathfinderId, updatedPathfinder, token);

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
