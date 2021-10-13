using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PathfinderHonorManager.Model;
using PathfinderHonorManager.Service.Interfaces;
using Incoming = PathfinderHonorManager.Dto.Incoming;

namespace PathfinderHonorManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PathfindersController : ControllerBase
    {

        private readonly IPathfinderService _pathfinderService;

        public PathfindersController(IPathfinderService pathfinderService)
        {
            _pathfinderService = pathfinderService;
        }

        // GET Pathfinders
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Pathfinder>>> GetAll(CancellationToken token)
        {
            var pathfinder = await _pathfinderService.GetAllAsync(token);

            if (pathfinder == default)
            {
                return NotFound();
            }

            return Ok(pathfinder);
        }
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

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PostAsync([FromBody] Incoming.PathfinderDto newPathfinder, CancellationToken token)
        {
            try
            {
                var pathfinder = await _pathfinderService.AddAsync(newPathfinder, token);

                return CreatedAtRoute(routeValues: GetByIdAsync(pathfinder.PathfinderID, token),
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

        [ApiExplorerSettings(IgnoreApi = true)]
        public void UpdateModelState(FluentValidation.ValidationException validationException)
        {
            foreach (var error in validationException.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
        }
    }
}
