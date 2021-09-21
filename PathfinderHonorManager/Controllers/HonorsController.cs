using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using PathfinderHonorManager.Models;
using PathfinderHonorManager.DataAccess;


namespace PathfinderHonorManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PathfinderHonorsController : ControllerBase
    {

        private readonly ILogger<PathfinderHonorsController> _logger;

        private readonly PostgresContext _context;

        public PathfinderHonorsController(PostgresContext context, ILogger<PathfinderHonorsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET Honors
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PathfinderHonor>>> GetPathfinderHonors()
        {
            return await _context.PathfinderHonors.ToListAsync();
        }
    }
}
