using TimeTracker.Web.Data;

namespace TimeTracker.Web.Features.Timer;

public class UpdateProductivityHandler(AppDbContext db)
{
    private readonly AppDbContext _db = db;

    public async Task HandleAsync(int entryId, int rating)
    {
        var entry = await _db.TimeEntries.FindAsync(entryId);
        if (entry is not null)
        {
            entry.ProductivityRating = rating;
            await _db.SaveChangesAsync();
        }
    }
}
