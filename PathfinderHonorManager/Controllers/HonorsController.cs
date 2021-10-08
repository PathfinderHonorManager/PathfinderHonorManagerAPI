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
    public class HonorsController : ControllerBase
    {

        private readonly IHonorService _honorService;

        public HonorsController(IHonorService honorService)
        {
            _honorService = honorService;
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Honor>>> GetHonors(CancellationToken token)
        {
            var honors = await _honorService.GetAllAsync(token);

            if (honors == default)
            {
                return NotFound();
            }

            return Ok(honors);
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken token)
        {

            var honor = await _honorService.GetByIdAsync(id, token);

            if (honor == default)
            {
                return NotFound();
            }

            return Ok(honor);

        }
    }
}
