using TimeTracker.Web.Data;

namespace TimeTracker.Web.Features.Journal.ListEntries;

public class DeleteJournalEntryHandler(AppDbContext db)
{
    public async Task HandleAsync(int id)
    {
        var entry = await db.JournalEntries.FindAsync(id);
        if (entry is not null)
        {
            db.JournalEntries.Remove(entry);
            await db.SaveChangesAsync();
        }
    }
}
