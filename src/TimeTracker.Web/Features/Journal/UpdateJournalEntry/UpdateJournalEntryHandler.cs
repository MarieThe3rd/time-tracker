using TimeTracker.Web.Data.Models;
using TimeTracker.Web.Data.Repositories;

namespace TimeTracker.Web.Features.Journal.UpdateJournalEntry;

public record UpdateJournalEntryInput(
    int Id,
    int JournalTypeId,
    string Title,
    string Body,
    DateOnly Date,
    int? LinkedTimeEntryId = null,
    int? JournalCategoryId = null);

public class UpdateJournalEntryHandler(IJournalEntryRepository journalRepo)
{
    public async Task<JournalEntry> HandleAsync(UpdateJournalEntryInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Title))
            throw new ArgumentException("Title cannot be empty.", nameof(input));

        var entry = await journalRepo.GetByIdAsync(input.Id)
            ?? throw new KeyNotFoundException($"Journal entry {input.Id} not found.");

        entry.JournalTypeId    = input.JournalTypeId;
        entry.JournalCategoryId = input.JournalCategoryId;
        entry.Title            = input.Title.Trim();
        entry.Body             = input.Body.Trim();
        entry.Date             = input.Date;
        entry.LinkedTimeEntryId = input.LinkedTimeEntryId;

        await journalRepo.UpdateAsync(entry);
        return entry;
    }
}
