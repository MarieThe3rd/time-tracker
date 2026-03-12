using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data.Models;

namespace TimeTracker.Web.Data.Repositories.Sql;

public class SqlJournalEntryRepository(AppDbContext db) : IJournalEntryRepository
{
    public async Task<JournalEntry?> GetByIdAsync(int id)
        => await db.JournalEntries.FindAsync(id);

    public async Task<List<JournalEntry>> GetFilteredAsync(
        JournalEntryType? type = null,
        DateOnly? from = null,
        DateOnly? to = null,
        bool includeLinkedTimeEntry = false)
    {
        var query = db.JournalEntries.AsQueryable();
        if (includeLinkedTimeEntry)
            query = query.Include(e => e.LinkedTimeEntry);
        if (type.HasValue)
            query = query.Where(e => e.Type == type.Value);
        if (from.HasValue)
            query = query.Where(e => e.Date >= from.Value);
        if (to.HasValue)
            query = query.Where(e => e.Date <= to.Value);

        return await query
            .OrderByDescending(e => e.Date)
            .ThenByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<JournalEntry>> GetRecentAsync(int count)
        => await db.JournalEntries
            .OrderByDescending(e => e.Date)
            .ThenByDescending(e => e.CreatedAt)
            .Take(count)
            .ToListAsync();

    public async Task<JournalEntry> AddAsync(JournalEntry entry)
    {
        db.JournalEntries.Add(entry);
        await db.SaveChangesAsync();
        return entry;
    }

    public async Task UpdateAsync(JournalEntry entry)
    {
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entry = await db.JournalEntries.FindAsync(id);
        if (entry is not null)
        {
            db.JournalEntries.Remove(entry);
            await db.SaveChangesAsync();
        }
    }
}
