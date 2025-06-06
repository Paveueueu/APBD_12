using APBD_12.Exceptions;
using APBD_12.Models;
using APBD_12.Models.DTOs;
using APBD_12.Services;
using Microsoft.AspNetCore.Mvc;

namespace APBD_12.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TripsController(IDbService dbService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetTrips([FromQuery] int pageNum = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var trips = await dbService.GetTripsAsync(pageNum, pageSize);
            return Ok(trips);
        }
        catch
        {
            return StatusCode(500);
        }
    }
    
    [HttpPut("{clientId}")]
    public async Task<IActionResult> SignClientForTrip([FromBody] ClientSigningDto data)
    {
        try
        {
            await dbService.SignClientForTripAsync(data);
            return Created();
        }
        catch (ClientAlreadyExistsException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (AlreadySignedException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (TripNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidTripException ex)
        {
            return BadRequest(ex.Message);
        }
        catch
        {
            return StatusCode(500);
        }
    }
}