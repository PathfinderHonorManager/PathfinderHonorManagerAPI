using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PathfinderHonorManager.Model;
using PathfinderHonorManager.DataAccess;
using PathfinderHonorManager.Service;
using PathfinderHonorManager.Service.Interfaces;
using Incoming = PathfinderHonorManager.Dto.Incoming;
using Outgoing = PathfinderHonorManager.Dto.Outgoing;
using Microsoft.AspNetCore.Http;
using AutoMapper;
using System.Threading;

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

                return CreatedAtRoute(
                    GetByIdAsync(pathfinder.PathfinderID, token),
                    pathfinder);
            }

            catch (FluentValidation.ValidationException ex)
            {
                return BadRequest(ex.Message);
            }

            catch (DbUpdateException ex)
            {
                return BadRequest(ex.Message);
            }

        }

    }
}
