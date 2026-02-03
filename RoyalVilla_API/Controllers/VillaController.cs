using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoyalVilla_API.Data;

namespace RoyalVilla_API.Controllers
{
    [Route("api/villa")]
    [ApiController]
    public class VillaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public VillaController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetVillas()
        {
            return Ok(await _context.Villas.ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetVillaById(int id)
        {
            try
            {
                if(id == 0)
                    return BadRequest($"Invalid villa ID {id}");

                var villa = await _context.Villas.FirstOrDefaultAsync(v => v.Id == id);
                if(villa == null)
                    return NotFound($"Villa with ID {id} not found");
                return Ok(villa);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"An error occured while retrieving villa with ID {id} : {ex.Message}");
            }
        }
    }
}
