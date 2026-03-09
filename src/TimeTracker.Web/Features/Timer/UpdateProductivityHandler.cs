using TimeTracker.Web.Data;

namespace TimeTracker.Web.Features.Timer;

public class UpdateProductivityHandler(AppDbContext db)
{
    private readonly AppDbContext _db = db;

    public async Task HandleAsync(int entryId, int rating)
    {
        if (rating < 1 || rating > 5)
            throw new ArgumentOutOfRangeException(nameof(rating), "Productivity rating must be between 1 and 5.");

        var entry = await _db.TimeEntries.FindAsync(entryId);
        if (entry is not null)
        {
            entry.ProductivityRating = rating;
            await _db.SaveChangesAsync();
        }
    }
}
