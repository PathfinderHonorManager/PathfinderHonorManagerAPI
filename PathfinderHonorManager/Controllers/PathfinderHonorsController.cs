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
    public class HonorsController : ControllerBase
    {

        private readonly ILogger<HonorsController> _logger;

        private readonly PostgresContext _context;

        public HonorsController(PostgresContext context, ILogger<HonorsController> logger)
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
    }
}
