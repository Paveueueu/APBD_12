using APBD_12.Models;
using APBD_12.Models.DTOs;

namespace APBD_12.Services;

public interface IDbService
{
    Task<TripsListDto> GetTripsAsync(int pageNum, int pageSize);
    Task DeleteClientAsync(int clientId);
    Task SignClientForTripAsync(ClientSigningDto data);
}