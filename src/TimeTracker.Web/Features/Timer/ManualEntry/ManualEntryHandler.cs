using TimeTracker.Web.Data;
using TimeTracker.Web.Data.Models;

namespace TimeTracker.Web.Features.Timer.ManualEntry;

public record ManualEntryInput(
    DateTime StartTime,
    DateTime EndTime,
    int? WorkCategoryId,
    string? Description,
    int? ProductivityRating);

public class ManualEntryHandler(AppDbContext db)
{
    private readonly AppDbContext _db = db;

    public async Task<TimeEntry> HandleAsync(ManualEntryInput input)
    {
        if (input.EndTime <= input.StartTime)
            throw new ArgumentException("End time must be after start time.", nameof(input));

        if (input.ProductivityRating.HasValue && (input.ProductivityRating.Value < 1 || input.ProductivityRating.Value > 5))
            throw new ArgumentOutOfRangeException(nameof(input), "Productivity rating must be between 1 and 5.");

        var entry = new TimeEntry
        {
            StartTime = input.StartTime,
            EndTime = input.EndTime,
            WorkCategoryId = input.WorkCategoryId,
            Description = input.Description,
            ProductivityRating = input.ProductivityRating
        };

        _db.TimeEntries.Add(entry);
        await _db.SaveChangesAsync();
        return entry;
    }
}
