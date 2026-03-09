using TimeTracker.Web.Data;
using TimeTracker.Web.Data.Models;

namespace TimeTracker.Web.Features.Journal.AddEntry;

public record AddJournalEntryInput(
    JournalEntryType Type,
    string Title,
    string Body,
    DateOnly? Date = null,
    int? LinkedTimeEntryId = null);

public class AddEntryHandler(AppDbContext db)
{
    public async Task<JournalEntry> HandleAsync(AddJournalEntryInput input)
    {
        var entry = new JournalEntry
        {
            Date = input.Date ?? DateOnly.FromDateTime(DateTime.Today),
            Type = input.Type,
            Title = input.Title.Trim(),
            Body = input.Body.Trim(),
            LinkedTimeEntryId = input.LinkedTimeEntryId,
            CreatedAt = DateTime.UtcNow
        };
        db.JournalEntries.Add(entry);
        await db.SaveChangesAsync();
        return entry;
    }
}
