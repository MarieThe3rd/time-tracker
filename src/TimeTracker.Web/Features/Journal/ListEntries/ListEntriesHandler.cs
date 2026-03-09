using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data;
using TimeTracker.Web.Data.Models;

namespace TimeTracker.Web.Features.Journal.ListEntries;

public record JournalFilter(
    JournalEntryType? Type = null,
    DateOnly? From = null,
    DateOnly? To = null);

public class ListEntriesHandler(AppDbContext db)
{
    public async Task<List<JournalEntry>> HandleAsync(JournalFilter filter)
    {
        if (filter.From.HasValue && filter.To.HasValue && filter.From.Value > filter.To.Value)
            throw new ArgumentException("From date must be on or before To date.");
        var query = db.JournalEntries
            .Include(e => e.LinkedTimeEntry)
            .AsQueryable();

        if (filter.Type.HasValue)
            query = query.Where(e => e.Type == filter.Type.Value);
        if (filter.From.HasValue)
            query = query.Where(e => e.Date >= filter.From.Value);
        if (filter.To.HasValue)
            query = query.Where(e => e.Date <= filter.To.Value);

        return await query.OrderByDescending(e => e.Date)
                          .ThenByDescending(e => e.CreatedAt)
                          .ToListAsync();
    }
}
