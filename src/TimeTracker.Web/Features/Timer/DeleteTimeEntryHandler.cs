using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data;

namespace TimeTracker.Web.Features.Timer;

public class DeleteTimeEntryHandler(AppDbContext db)
{
    private readonly AppDbContext _db = db;

    public async Task HandleAsync(int id)
    {
        var entry = await _db.TimeEntries.FindAsync(id);
        if (entry is not null)
        {
            _db.TimeEntries.Remove(entry);
            await _db.SaveChangesAsync();
        }
    }
}
