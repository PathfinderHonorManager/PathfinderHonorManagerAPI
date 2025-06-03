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
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace PathfinderHonorManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize("ReadHonors")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public class HonorsController : CustomApiController
    {
        private readonly IHonorService _honorService;
        private readonly ILogger<HonorsController> _logger;

        public HonorsController(IHonorService honorService, ILogger<HonorsController> logger)
        {
            _honorService = honorService;
            _logger = logger;
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
            _logger.LogInformation("Getting all honors");
            var honors = await this._honorService.GetAllAsync(token);

            if (honors == default)
            {
                _logger.LogWarning("No honors found");
                return NotFound();
            }

            _logger.LogInformation("Retrieved {Count} honors", honors.Count);
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
        [HttpGet("{id:guid}", Name = "GetHonorById")]
        public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken token)
        {
            _logger.LogInformation("Getting honor with ID {HonorId}", id);
            var honor = await this._honorService.GetByIdAsync(id, token);

            if (honor == default)
            {
                _logger.LogWarning("Honor with ID {HonorId} not found", id);
                return NotFound();
            }

            _logger.LogInformation("Retrieved honor with ID {HonorId}", id);
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
            _logger.LogInformation("Creating new honor");
            try
            {
                var honor = await _honorService.AddAsync(newHonor, token);

                _logger.LogInformation("Created honor with ID {HonorId}", honor.HonorID);
                return CreatedAtRoute(
                    "GetHonorById",
                    new { id = honor.HonorID },
                    honor);
            }
            catch (FluentValidation.ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation failed while creating honor");
                UpdateModelState(ex);
                return ValidationProblem(ModelState);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while creating honor");
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
            _logger.LogInformation("Updating honor with ID {HonorId}", id);
            var honor = await _honorService.GetByIdAsync(id, token);

            if (honor == default)
            {
                _logger.LogWarning("Honor with ID {HonorId} not found", id);
                return NotFound();
            }

            try
            {
                await _honorService.UpdateAsync(id, updatedHonor, token);

                honor = await _honorService.GetByIdAsync(id, token);
                _logger.LogInformation("Updated honor with ID {HonorId}", id);

                return honor != default
                    ? Ok(honor)
                    : NotFound();
            }
            catch (FluentValidation.ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation failed while updating honor with ID {HonorId}", id);
                UpdateModelState(ex);
                return ValidationProblem(ModelState);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while updating honor with ID {HonorId}", id);
                return ValidationProblem(ex.Message);
            }
        }
    }
}
