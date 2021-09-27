using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using PathfinderHonorManager.Model;
using PathfinderHonorManager.DataAccess;
using System;

namespace PathfinderHonorManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PathfinderHonorsStatusController : ControllerBase
    {

        private readonly ILogger<HonorsController> _logger;

        private readonly PathfinderContext _context;

        public PathfinderHonorsStatusController(PathfinderContext context, ILogger<HonorsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET Honors
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PathfinderHonorStatus>>> GetHonors()
        {
            return await _context.PathfinderHonorStatuses.ToListAsync();
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<PathfinderHonorStatus>> GetByIdAsync(Guid id)
        {

            return await _context.PathfinderHonorStatuses.FindAsync(id);

        }
    }
}
