using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PathfinderHonorManager.Service.Interfaces;
using Incoming = PathfinderHonorManager.Dto.Incoming;
using Outgoing = PathfinderHonorManager.Dto.Outgoing;

namespace PathfinderHonorManager.Controllers
{
    [ApiController]
    [Route("api/pathfinders/{pathfinderId:guid}/[controller]")]
    public class PathfinderHonorsController : ControllerBase
    {
        private readonly IPathfinderHonorService _PathfinderHonorService;

        public PathfinderHonorsController(IPathfinderHonorService PathfinderHonorService)
        {
            _PathfinderHonorService = PathfinderHonorService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Outgoing.PathfinderHonorDto>>> GetAll(Guid pathfinderId, CancellationToken token)
        {
            var pathfinder = await _PathfinderHonorService.GetAllAsync(pathfinderId, token);

            if (pathfinder == default)
            {
                return NotFound();
            }

            return Ok(pathfinder);
        }

        [HttpGet("{honorId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByIdAsync(Guid pathfinderId, Guid honorId, CancellationToken token)
        {

            var pathfinder = await _PathfinderHonorService.GetByIdAsync(pathfinderId, honorId, token);

            if (pathfinder == default)
            {
                return NotFound();
            }

            return Ok(pathfinder);

        }

        [HttpPost]
        [ProducesResponseType(typeof(Dto.Outgoing.PathfinderHonorDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PostAsync(Guid pathfinderId, [FromBody] Incoming.PostPathfinderHonorDto newPathfinderHonor, CancellationToken token)
        {
            try
            {
                var pathfinderHonor = await _PathfinderHonorService.AddAsync(pathfinderId, newPathfinderHonor, token);

                return CreatedAtRoute(routeValues: GetByIdAsync(pathfinderHonor.PathfinderID, pathfinderHonor.HonorID, token),
                                      pathfinderHonor);
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

        [HttpPut("{honorId:guid}")]
        [ProducesResponseType(typeof(Dto.Outgoing.PathfinderHonorDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutAsync(Guid pathfinderId, Guid honorId,[FromBody] Incoming.PutPathfinderHonorDto incomingPathfinderHonor, CancellationToken token)
        {
            try
            {
                var pathfinderHonor = await _PathfinderHonorService.UpdateAsync(pathfinderId, honorId, incomingPathfinderHonor, token);

                return pathfinderHonor != default
                ? Ok(pathfinderHonor)
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