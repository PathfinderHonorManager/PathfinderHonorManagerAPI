using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PathfinderHonorManager.Model;
using PathfinderHonorManager.Service.Interfaces;

namespace PathfinderHonorManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Authorize("ReadHonors")]
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
            var honors = await this._honorService.GetAllAsync(token);

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
            var honor = await this._honorService.GetByIdAsync(id, token);

            if (honor == default)
            {
                return NotFound();
            }

            return Ok(honor);
        }
    }
}
