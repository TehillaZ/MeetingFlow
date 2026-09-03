using DataAccessor.Data;
using DataAccessor.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessor.Repositories;

public class VenuesRepository
{
    readonly MeetingFlowDbContext _db;
    public VenuesRepository(MeetingFlowDbContext db) => _db = db;

    public Task<List<Venue>> GetAllAsync() =>
        _db.Venues.ToListAsync();

    public async Task<Venue> CreateAsync(Venue venue)
    {
        _db.Venues.Add(venue);
        await _db.SaveChangesAsync();
        return venue;
    }
}
