namespace APBD_12.Models.DTOs;


public class TripDto
{
    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public DateTime DateFrom { get; set; }

    public DateTime DateTo { get; set; }

    public int MaxPeople { get; set; }

    public virtual ICollection<ClientDto> Clients { get; set; } = new List<ClientDto>();

    public virtual ICollection<CountryDto> Countries { get; set; } = new List<CountryDto>();
}
