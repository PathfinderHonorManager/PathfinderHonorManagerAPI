using System.Collections.Generic;
using System;
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
    public class PathfinderHonorsController : ControllerBase
    {
        private readonly IPathfinderHonorService _PathfinderHonorService;

        public PathfinderHonorsController(IPathfinderHonorService PathfinderHonorService)
        {
            _PathfinderHonorService = PathfinderHonorService;
        }


        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken token)
        {

            var pathfinder = await _PathfinderHonorService.GetByIdAsync(id, token);

            if (pathfinder == default)
            {
                return NotFound();
            }

            return Ok(pathfinder);

        }

        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] Incoming.PathfinderHonorDto newPathfinderHonor, CancellationToken token)
        {
            var pathfinderHonor = await _PathfinderHonorService.AddAsync(newPathfinderHonor, token);

            return CreatedAtRoute(
                GetByIdAsync(pathfinderHonor.PathfinderHonorID, token),
                pathfinderHonor);
        }


    }
}
