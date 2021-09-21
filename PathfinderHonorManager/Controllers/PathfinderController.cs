using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace PathfinderHonorManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PathfindersController : ControllerBase
    {

        private readonly ILogger<PathfindersController> _logger;

        private readonly PostgresContext _context;

        public PathfindersController(PostgresContext context, ILogger<PathfindersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET Pathfinders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Pathfinder>>> GetPathfinders()
        {
            return await _context.Pathfinders.ToListAsync();
        }
    }
}
