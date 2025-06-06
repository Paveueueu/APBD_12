using APBD_12.Data;
using APBD_12.Models.DTOs;
using APBD_12.Exceptions;
using APBD_12.Models;
using Microsoft.EntityFrameworkCore;


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
            .OrderBy(trip => trip.DateFrom)
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
        await using var transaction = await context.Database.BeginTransactionAsync();
    
        try
        {
            // Check if a client with this PESEL already exists
            var clientExists = await context.Clients
                .AnyAsync(c => c.Pesel == data.Pesel);
            if (clientExists)
                throw new ClientAlreadyExistsException();

            // Create new client otherwise
            var client = new Client
            {
                FirstName = data.FirstName,
                LastName = data.LastName,
                Email = data.Email,
                Telephone = data.Telephone,
                Pesel = data.Pesel
            };
            context.Clients.Add(client);
            await context.SaveChangesAsync();

            // Check if the trip exists
            var trip = await context.Trips
                .FirstOrDefaultAsync(t => t.IdTrip == data.IdTrip);
            if (trip == null)
                throw new TripNotFoundException();
        
            // Check if the trip is still available
            if (trip.DateFrom <= DateTime.Now)
                throw new InvalidTripException();

            // Check if the client already signed for the trip
            var isSigned = await context.ClientTrips
                .AnyAsync(clientTrip => clientTrip.IdClient == client.IdClient 
                                        && clientTrip.IdTrip == data.IdTrip);
            if (isSigned)
                throw new AlreadySignedException();

            // Check if there is space on the trip
            var countPeople = await context.ClientTrips
                .CountAsync(clientTrip => clientTrip.IdTrip == data.IdTrip);
            if (countPeople >= trip.MaxPeople)
                throw new TripFullException();

            // Register the client for the trip
            context.ClientTrips.Add(new ClientTrip
            {
                IdClient = client.IdClient,
                IdTrip = data.IdTrip,
                RegisteredAt = DateTime.Now,
                PaymentDate = data.PaymentDate
            });

            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}