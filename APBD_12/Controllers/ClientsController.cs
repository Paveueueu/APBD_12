using APBD_12.Exceptions;
using APBD_12.Services;
using Microsoft.AspNetCore.Mvc;

namespace APBD_12.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientsController(IDbService dbService) : ControllerBase
{
    [HttpDelete("{idClient:int}")]
    public async Task<IActionResult> DeleteClient(int idClient)
    {
        try
        {
            await dbService.DeleteClientAsync(idClient);
            return NoContent();
        }
        catch (CannotDeleteClientException ex)
        {
            return BadRequest(ex.Message);
        }
        catch
        {
            return StatusCode(500);
        }
    }
}