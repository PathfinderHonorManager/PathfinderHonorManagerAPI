using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PathfinderHonorManager.Dto.Incoming;
using PathfinderHonorManager.Service.Interfaces;
using Incoming = PathfinderHonorManager.Dto.Incoming;
using Outgoing = PathfinderHonorManager.Dto.Outgoing;

namespace PathfinderHonorManager.Controllers
{
    [ApiController]
    [Route("api/pathfinders")]
    [Authorize("ReadPathfinders")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public class PathfinderHonorsController : CustomApiController
    {
        private readonly IPathfinderHonorService _pathfinderHonorService;

        public PathfinderHonorsController(IPathfinderHonorService pathfinderHonorService)
        {
            _pathfinderHonorService = pathfinderHonorService;
        }

        // GET Pathfinders/{id}/PathfinderHonors
        /// <summary>
        /// Get Pathfinder honors by Pathfinder Id
        /// </summary>
        /// <param name="pathfinderId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpGet("{pathfinderId:guid}/PathfinderHonors", Name = "GetAllPathfinderHonors")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Outgoing.PathfinderHonorDto>>> GetAll(Guid pathfinderId, CancellationToken token)
        {
            var pathfinder = await _pathfinderHonorService.GetAllAsync(pathfinderId, token);

            if (pathfinder == default)
            {
                return NotFound();
            }

            return Ok(pathfinder);
        }

        // GET Pathfinders/PathfinderHonors?status={status}
        /// <summary>
        /// Get Pathfinder honors by status
        /// </summary>
        /// <param name="status"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpGet("PathfinderHonors", Name = "GetAllPathfinderHonorsByStatus")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Outgoing.PathfinderHonorDto>>> GetAllByStatus([FromQuery] string status, CancellationToken token)
        {
            var pathfinder = await _pathfinderHonorService.GetAllByStatusAsync(status, token);

            if (pathfinder == default)
            {
                return NotFound();
            }

            return Ok(pathfinder);
        }

        // GET Pathfinders/{id}/PathfinderHonors/{honorId}
        /// <summary>
        /// Get PathfinderHonor by Id
        /// </summary>
        /// <param name="pathfinderId"></param>
        /// <param name="honorId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpGet("{pathfinderId:guid}/PathfinderHonors/{honorId:guid}", Name = "GetPathfinderHonorById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByIdAsync(Guid pathfinderId, Guid honorId, CancellationToken token)
        {
            var pathfinder = await _pathfinderHonorService.GetByIdAsync(pathfinderId, honorId, token);

            if (pathfinder == default)
            {
                return NotFound();
            }

            return Ok(pathfinder);
        }

        // POST Pathfinders/{id}/PathfinderHonors
        /// <summary>
        /// Add a new PathfinderHonor
        /// </summary>
        /// <param name="pathfinderId"></param>
        /// <param name="newPathfinderHonor"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("{pathfinderId:guid}/PathfinderHonors", Name = "PostPathfinderHonor")]
        [Authorize("UpdatePathfinders")]
        [ProducesResponseType(typeof(Outgoing.PathfinderHonorDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PostAsync(Guid pathfinderId, [FromBody] Incoming.PostPathfinderHonorDto newPathfinderHonor, CancellationToken token)
        {
            try
            {
                var pathfinderHonor = await _pathfinderHonorService.AddAsync(pathfinderId, newPathfinderHonor, token);

                return CreatedAtRoute(
                    routeValues: GetByIdAsync(pathfinderHonor.PathfinderID, pathfinderHonor.HonorID, token),
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

        // POST Pathfinders/PathfinderHonors
        /// <summary>
        /// Add new PathfinderHonors in bulk
        /// </summary>
        /// <param name="bulkData"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("PathfinderHonors", Name = "BulkPostPathfinderHonors")]
        [Authorize("UpdatePathfinders")]
        [ProducesResponseType(StatusCodes.Status207MultiStatus)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> BulkPostAsync([FromBody] IEnumerable<BulkPostPathfinderHonorDto> bulkData, CancellationToken token)
        {
            var responses = new List<object>();

            foreach (var data in bulkData)
            {
                foreach (var honor in data.Honors)
                {
                    try
                    {
                        var pathfinderHonor = await _pathfinderHonorService.AddAsync(data.PathfinderID, honor, token);

                        responses.Add(new
                        {
                            status = StatusCodes.Status201Created,
                            pathfinderHonor
                        });
                    }
                    catch (FluentValidation.ValidationException ex)
                    {
                        UpdateModelState(ex);
                        responses.Add(new
                        {
                            status = StatusCodes.Status400BadRequest,
                            error = ex.Message
                        });
                    }
                    catch (DbUpdateException ex)
                    {
                        responses.Add(new
                        {
                            status = StatusCodes.Status400BadRequest,
                            error = ex.Message
                        });
                    }
                }
            }

            return StatusCode(StatusCodes.Status207MultiStatus, responses);
        }

        // PUT Pathfinders/{id}/PathfinderHonors/{honorId}
        /// <summary>
        /// Update a PathfinderHonor
        /// </summary>
        /// <param name="pathfinderId"></param>
        /// <param name="honorId"></param>
        /// <param name="incomingPathfinderHonor"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPut("{pathfinderId:guid}/PathfinderHonors/{honorId:guid}", Name = "PutPathfinderHonor")]
        [Authorize("UpdatePathfinders")]
        [ProducesResponseType(typeof(Outgoing.PathfinderHonorDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutAsync(Guid pathfinderId, Guid honorId, [FromBody] Incoming.PutPathfinderHonorDto incomingPathfinderHonor, CancellationToken token)
        {
            try
            {
                var pathfinderHonor = await _pathfinderHonorService.UpdateAsync(pathfinderId, honorId, incomingPathfinderHonor, token);

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

        // PUT Pathfinders/PathfinderHonors
        /// <summary>
        /// Update multiple PathfinderHonor objects at once
        /// </summary>
        /// <param name="bulkData"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPut("PathfinderHonors", Name = "BulkPutPathfinderHonors")]
        [Authorize("UpdatePathfinders")]
        [ProducesResponseType(StatusCodes.Status207MultiStatus)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> BulkPutAsync([FromBody] IEnumerable<BulkPutPathfinderHonorDto> bulkData, CancellationToken token)
        {
            var responses = new List<object>();

            foreach (var data in bulkData)
            {
                foreach (var honor in data.Honors)
                {
                    try
                    {
                        var pathfinderHonor = await _pathfinderHonorService.UpdateAsync(data.PathfinderID, honor.HonorID, honor, token);

                        responses.Add(new
                        {
                            status = pathfinderHonor != default ? StatusCodes.Status200OK : StatusCodes.Status404NotFound,
                            pathfinderHonor
                        });
                    }
                    catch (FluentValidation.ValidationException ex)
                    {
                        UpdateModelState(ex);
                        responses.Add(new
                        {
                            status = StatusCodes.Status400BadRequest,
                            error = ex.Message
                        });
                    }
                    catch (DbUpdateException ex)
                    {
                        responses.Add(new
                        {
                            status = StatusCodes.Status400BadRequest,
                            error = ex.Message
                        });
                    }
                }
            }

            return StatusCode(StatusCodes.Status207MultiStatus, responses);
        }

    }
}