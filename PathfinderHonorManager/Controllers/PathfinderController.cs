using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PathfinderHonorManager.Models;
using PathfinderHonorManager.DataAccess;
using PathfinderHonorManager.Service;
using PathfinderHonorManager.Service.Interfaces;
using PathfinderHonorManager.Dto;
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
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Pathfinder>>> GetPathfinders(CancellationToken token)
        {
            var pathfinder = await _pathfinderService.GetAllAsync(token);

            if (pathfinder == default)
            {
                return NotFound();
            }

            return Ok(pathfinder);
        }

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

    }
}
