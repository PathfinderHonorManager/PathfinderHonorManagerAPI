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
    [Authorize("ReadHonors")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public class HonorsController : ControllerBase
    {
        private readonly IHonorService _honorService;

        public HonorsController(IHonorService honorService)
        {
            _honorService = honorService;
        }

        // GET Honors
        /// <summary>
        /// Get all Honors
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
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

        // GET Honors/{id}
        /// <summary>
        /// Get an Honor by Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
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
