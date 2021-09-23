using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using PathfinderHonorManager.Models;
using PathfinderHonorManager.DataAccess;
using System;

namespace PathfinderHonorManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HonorsController : ControllerBase
    {

        private readonly ILogger<HonorsController> _logger;

        private readonly PathfinderContext _context;

        public HonorsController(PathfinderContext context, ILogger<HonorsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET Honors
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Honor>>> GetHonors()
        {
            return await _context.Honors.ToListAsync();
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<Honor>> GetByIdAsync(Guid id)
        {

            return await _context.Honors.FindAsync(id);

        }
    }
}
