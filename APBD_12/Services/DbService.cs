using APBD_12.Data;
using APBD_12.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using APBD_12.Exceptions;
using APBD_12.Models;


namespace APBD_12.Services;

public class DbService(TripsContext context) : IDbService
{
    public async Task<TripsListDto> GetTripsAsync(int pageNum, int pageSize)
    {
        var countTrips = await context.Trips.CountAsync();
        var allPages = (int) Math.Ceiling(countTrips / (double) pageSize);
        
        var trips = await context.Trips
            .Include(trip => trip.IdCountries)
            .Include(trip => trip.ClientTrips)
            .ThenInclude(clientTrip => clientTrip.IdClientNavigation)
            .OrderByDescending(trip => trip.DateFrom)
            .Skip((pageNum - 1) * pageSize)
            .Take(pageSize)
            .Select(trip => new TripDto
            {
                Name = trip.Name,
                Description = trip.Description,
                DateFrom = trip.DateFrom,
                DateTo = trip.DateTo,
                MaxPeople = trip.MaxPeople,
                Countries = trip.IdCountries.Select(country => new CountryDto
                {
                    Name = country.Name
                }).ToList(),
                Clients = trip.ClientTrips.Select(clientTrip => new ClientDto
                {
                    FirstName = clientTrip.IdClientNavigation.FirstName,
                    LastName = clientTrip.IdClientNavigation.LastName
                }).ToList()
            })
            .ToListAsync();
        
        return new TripsListDto
        {
            AllPages = allPages,
            PageNum = pageNum,
            PageSize = pageSize,
            Trips = trips,
        };
    }

    public async Task DeleteClientAsync(int clientId)
    {
        var client = await context.Clients
            .Include(c => c.ClientTrips)
            .FirstOrDefaultAsync(c => c.IdClient == clientId);

        if (client == null)
            throw new ClientNotFoundException();

        if (client.ClientTrips != null && client.ClientTrips.Count != 0)
            throw new CannotDeleteClientException();
        
        context.Clients.Remove(client);
        await context.SaveChangesAsync();
    }

    public async Task SignClientForTripAsync(ClientSigningDto data)
    {
        throw new NotImplementedException();
    }
}