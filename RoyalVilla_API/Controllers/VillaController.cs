using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
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
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<VillaDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<VillaDTO>>>> GetVillas()
        {
            var villas = await _context.Villas.ToListAsync();

            var dtoVillaResponse = _mapper.Map<List<VillaDTO>>(villas);
            var response = ApiResponse<IEnumerable<VillaDTO>>.Ok(dtoVillaResponse, "Villas retrieved successfully");
            return Ok(response);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<VillaDTO>>> GetVillaById(int id)
        {
            try
            {

                if (id <= 0)
                    return BadRequest(ApiResponse<object>.BadRequest("Villa Id must be greater than 0"));

                var villa = await _context.Villas.FirstOrDefaultAsync(v => v.Id == id);
                if (villa == null)
                    return NotFound(ApiResponse<VillaDTO>.NotFound($"Villa with ID {id} not found"));

                var response = ApiResponse<VillaDTO>.Ok(_mapper.Map<VillaDTO>(villa), "Villa retrieved successfully");
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(StatusCodes.Status500InternalServerError
                    , "An error occured while retreving the villa", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<VillaDTO>>> CreateVilla(VillaCreateDTO villaDto)
        {
            try
            {
                if (villaDto == null)
                    return BadRequest(ApiResponse<object>.BadRequest("Villa data is required"));

                var duplicateVilla = await _context.Villas.AnyAsync(v => v.Name.ToLower() == villaDto.Name.ToLower());
                if (duplicateVilla)
                    return Conflict(ApiResponse<object>.Confilct($"A villa with the name '{villaDto.Name}' already exist"));

                var villa = _mapper.Map<Villa>(villaDto);

                await _context.Villas.AddAsync(villa);
                await _context.SaveChangesAsync();

                var response = ApiResponse<VillaDTO>.CreatedAt(_mapper.Map<VillaDTO>(villa), "Villa created successfully");
                return CreatedAtAction(nameof(GetVillaById),new {Id = villa.Id},response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(StatusCodes.Status500InternalServerError
                    , "An error occured while creating the villa", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError,errorResponse);
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<VillaDTO>>> UpdateVilla(int id, VillaUpdateDTO villaDto)
        {
            try
            {
                if (villaDto == null)
                    return BadRequest(ApiResponse<object>.BadRequest($"Villa data is required"));

                var exitingVilla = await _context.Villas.FirstOrDefaultAsync(v => v.Id == id);
                if (exitingVilla is null)
                    return NotFound(ApiResponse<object>.NotFound($"Villa with {id} was not found"));

                var duplicateVilla = await _context.Villas.AnyAsync(v => v.Name.ToLower() == villaDto.Name.ToLower()
                && v.Id != id);

                if (duplicateVilla)
                    return Conflict(ApiResponse<object>.Confilct($"A villa with the name '{villaDto.Name}' already exist"));

                _mapper.Map(villaDto, exitingVilla);
                exitingVilla.UpdatedDate = DateTime.Now;

                await _context.SaveChangesAsync();

                var resultDto = _mapper.Map<VillaDTO>(villaDto);
                resultDto.Id = id;

                var response = ApiResponse<VillaDTO>.Ok(resultDto, "Villa updated successfully");
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(StatusCodes.Status500InternalServerError
                    , "An error occured while updating the villa", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError,errorResponse);
            }
        }


        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteVilla(int id)
        {
            try
            {
                var exitingVilla = await _context.Villas.FirstOrDefaultAsync(v => v.Id == id);
                if (exitingVilla is null)
                    return NotFound(ApiResponse<object>.NotFound($"Villa with {id} was not found"));

                _context.Villas.Remove(exitingVilla);
                await _context.SaveChangesAsync();
                
                var response = ApiResponse<object>.NoContent("Villa deleted successfully");
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(StatusCodes.Status500InternalServerError
                    , "An error occured while deleting the villa", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }
    }
}
