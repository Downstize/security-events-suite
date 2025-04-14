using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Processor.Data;

namespace Processor.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IncidentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public IncidentsController(ApplicationDbContext context)
    {
        _context = context;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetIncidents(int page = 1, int pageSize = 10)
    {
        var query = _context.Incidents
            .Include(i => i.Events)
            .OrderByDescending(i => i.Time);

        var incidents = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(incidents);
    }
}