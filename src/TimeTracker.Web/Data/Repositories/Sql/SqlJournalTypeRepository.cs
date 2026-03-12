using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data.Models;

namespace TimeTracker.Web.Data.Repositories.Sql;

public class SqlJournalTypeRepository(AppDbContext db) : IJournalTypeRepository
{
    public async Task<JournalType?> GetByIdAsync(int id)
        => await db.JournalTypes.FindAsync(id);

    public async Task<List<JournalType>> GetAllAsync()
        => await db.JournalTypes.OrderBy(t => t.Id).ToListAsync();

    public async Task<JournalType> AddAsync(JournalType journalType)
    {
        db.JournalTypes.Add(journalType);
        await db.SaveChangesAsync();
        return journalType;
    }

    public async Task UpdateAsync(JournalType journalType)
    {
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var type = await db.JournalTypes.FindAsync(id);
        if (type is null) return;
        if (type.IsSystem)
            throw new InvalidOperationException($"Cannot delete system journal type '{type.Name}'.");
        db.JournalTypes.Remove(type);
        await db.SaveChangesAsync();
    }
}
