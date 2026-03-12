using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data.Models;

namespace TimeTracker.Web.Data.Repositories.Sql;

public class SqlTimeEntryRepository(AppDbContext db) : ITimeEntryRepository
{
    public async Task<TimeEntry?> GetByIdAsync(int id)
        => await db.TimeEntries.FindAsync(id);

    public async Task<List<TimeEntry>> GetByDateRangeAsync(DateTime start, DateTime end, bool includeCategory = false)
    {
        var query = db.TimeEntries.AsQueryable();
        if (includeCategory)
            query = query.Include(e => e.WorkCategory);

        return await query
            .Where(e => e.StartTime >= start && e.StartTime <= end && e.EndTime != null)
            .OrderBy(e => e.StartTime)
            .ToListAsync();
    }

    public async Task<List<TimeEntry>> GetTodayAsync(bool includeCategory = false)
    {
        var todayLocal = DateOnly.FromDateTime(DateTime.Now);
        var startUtc = todayLocal.ToDateTime(TimeOnly.MinValue, DateTimeKind.Local).ToUniversalTime();
        var endUtc = todayLocal.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Local).ToUniversalTime();

        var query = db.TimeEntries.AsQueryable();
        if (includeCategory)
            query = query.Include(e => e.WorkCategory);

        return await query
            .Where(e => e.StartTime >= startUtc && e.StartTime <= endUtc)
            .OrderByDescending(e => e.StartTime)
            .ToListAsync();
    }

    public async Task<List<TimeEntry>> GetRecentAsync(int count, bool includeCategory = false)
    {
        var query = db.TimeEntries.AsQueryable();
        if (includeCategory)
            query = query.Include(e => e.WorkCategory);

        return await query
            .OrderByDescending(e => e.StartTime)
            .Take(count)
            .ToListAsync();
    }

    public async Task<TimeEntry> AddAsync(TimeEntry entry)
    {
        db.TimeEntries.Add(entry);
        await db.SaveChangesAsync();
        return entry;
    }

    public async Task UpdateAsync(TimeEntry entry)
    {
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entry = await db.TimeEntries.FindAsync(id);
        if (entry is not null)
        {
            db.TimeEntries.Remove(entry);
            await db.SaveChangesAsync();
        }
    }
}
