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
    [Authorize("ReadHonors")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public class HonorsController : ApiController
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

            return Ok(new { id = honor.HonorID, honor });
        }

        // POST Honors
        /// <summary>
        /// Adds a new Honor
        /// </summary>
        /// <param name="newHonor"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [Authorize("CreateHonors")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost]
        public async Task<ActionResult<Honor>> Post([FromBody] Incoming.HonorDto newHonor, CancellationToken token)
        {    
            try
            {
                var honor = await _honorService.AddAsync(newHonor, token);

                return CreatedAtRoute(
                    routeValues: GetByIdAsync(honor.HonorID, token),
                    honor);
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

        // PUT Honors/{id}
        /// <summary>
        /// Updates an Honor by Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="updatedHonor"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [Authorize("UpdateHonors")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Put(Guid id, [FromBody] Incoming.HonorDto updatedHonor, CancellationToken token)
        {
            var honor = await _honorService.GetByIdAsync(id, token);

            if (honor == default)
            {
                return NotFound();
            }

            await _honorService.UpdateAsync(id, updatedHonor, token);

            honor = await _honorService.GetByIdAsync(id, token);

            return honor != default
                ? Ok(honor)
                : NotFound();
        }
    }
}
