using TimeTracker.Web.Data.Models;
using TimeTracker.Web.Data.Repositories;

namespace TimeTracker.Web.Features.Journal.AddEntry;

public record AddJournalEntryInput(
    JournalEntryType Type,
    string Title,
    string Body,
    DateOnly? Date = null,
    int? LinkedTimeEntryId = null);

public class AddEntryHandler(IJournalEntryRepository journalRepo)
{
    public async Task<JournalEntry> HandleAsync(AddJournalEntryInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Title))
            throw new ArgumentException("Title cannot be empty.", nameof(input));
        var entry = new JournalEntry
        {
            Date = input.Date ?? DateOnly.FromDateTime(DateTime.Today),
            Type = input.Type,
            Title = input.Title.Trim(),
            Body = input.Body.Trim(),
            LinkedTimeEntryId = input.LinkedTimeEntryId,
            CreatedAt = DateTime.UtcNow
        };
        return await journalRepo.AddAsync(entry);
    }
}
