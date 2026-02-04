using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoyalVilla_API.Data;
using RoyalVilla_API.Models;
using RoyalVilla_API.Models.DTO;

namespace RoyalVilla_API.Controllers
{
    [Route("api/villa")]
    [ApiController]
    public class VillaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public VillaController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
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


        [HttpPost]
        public async Task<IActionResult> CreateVilla(VillaCreateDTO villaDto)
        {
            try
            {
                if (villaDto == null)
                    return BadRequest($"Villa data is required");

                var duplicateVilla = await _context.Villas.AnyAsync(v => v.Name.ToLower() == villaDto.Name.ToLower());
                if (duplicateVilla)
                    return Conflict($"A villa with the name '{villaDto.Name}' already exist");

                var villa = _mapper.Map<Villa>(villaDto);

                await _context.Villas.AddAsync(villa);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetVillaById),new {Id = villa.Id},villa);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"An error occured while creating the villa {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult>UpdateVilla(int id, VillaUpdateDTO villaDto)
        {
            try
            {
                if (villaDto == null)
                    return BadRequest($"Villa data is required");

                var exitingVilla = await _context.Villas.FirstOrDefaultAsync(v => v.Id == id);
                if (exitingVilla is null)
                    return NotFound($"Villa with {id} was not found");

                var duplicateVilla = await _context.Villas.AnyAsync(v => v.Name.ToLower() == villaDto.Name.ToLower()
                && v.Id != id);

                if (duplicateVilla)
                    return Conflict($"A villa with the name '{villaDto.Name}' already exist");

                _mapper.Map(villaDto, exitingVilla);
                exitingVilla.UpdatedDate = DateTime.Now;

                await _context.SaveChangesAsync();

                return Ok(villaDto);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"An error occured while updateing the villa {ex.Message}");
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVilla(int id)
        {
            try
            {
                var exitingVilla = await _context.Villas.FirstOrDefaultAsync(v => v.Id == id);
                if (exitingVilla is null)
                    return NotFound($"Villa with {id} was not found");

                _context.Villas.Remove(exitingVilla);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"An error occured while deleting the villa {ex.Message}");
            }
        }
    }
}
