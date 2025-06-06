namespace APBD_12.Models.DTOs;

public class TripsListDto
{
    public int PageNum { get; set; }
    public int PageSize { get; set; }
    public int AllPages { get; set; }
    
    public required IEnumerable<TripDto> Trips { get; set; }
}