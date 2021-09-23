using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using PathfinderHonorManager.Models;
using PathfinderHonorManager.DataAccess;
using System.Threading;
using System;

namespace PathfinderHonorManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PathfinderHonorsController : ControllerBase
    {

        private readonly ILogger<PathfinderHonorsController> _logger;

        private readonly PathfinderContext _context;

        public PathfinderHonorsController(PathfinderContext context, ILogger<PathfinderHonorsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET Honors
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PathfinderHonor>>> GetPathfinderHonors()
        {
            return await _context.PathfinderHonors.Include(h => h.Honor)
                .ToListAsync();
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<PathfinderHonor>> GetPathinderHonor(Guid id)
        {

            return await _context.PathfinderHonors.Include(h => h.Honor)
                .SingleOrDefaultAsync(p => p.PathfinderHonorID == id);

        }

    }
}
