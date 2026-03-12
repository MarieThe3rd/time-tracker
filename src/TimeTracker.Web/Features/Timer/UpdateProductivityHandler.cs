using TimeTracker.Web.Data.Repositories;

namespace TimeTracker.Web.Features.Timer;

public class UpdateProductivityHandler(ITimeEntryRepository timeEntryRepo)
{
    public async Task HandleAsync(int entryId, int rating)
    {
        if (rating < 1 || rating > 5)
            throw new ArgumentOutOfRangeException(nameof(rating), "Productivity rating must be between 1 and 5.");

        var entry = await timeEntryRepo.GetByIdAsync(entryId);
        if (entry is not null)
        {
            if (entry.IsBreak)
                return;

            entry.ProductivityRating = rating;
            await timeEntryRepo.UpdateAsync(entry);
        }
    }
}
