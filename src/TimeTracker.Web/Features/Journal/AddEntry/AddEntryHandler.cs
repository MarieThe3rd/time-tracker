using TimeTracker.Web.Data.Models;
using TimeTracker.Web.Data.Repositories;

namespace TimeTracker.Web.Features.Journal.AddEntry;

public record AddJournalEntryInput(
    int JournalTypeId,
    string Title,
    string Body,
    DateOnly? Date = null,
    int? LinkedTimeEntryId = null,
    int? JournalCategoryId = null);

public class AddEntryHandler(IJournalEntryRepository journalRepo, IJournalCategoryRepository categoryRepo)
{
    public async Task<JournalEntry> HandleAsync(AddJournalEntryInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Title))
            throw new ArgumentException("Title cannot be empty.", nameof(input));

        int? resolvedCategoryId = input.JournalCategoryId;
        if (resolvedCategoryId.HasValue && await categoryRepo.GetByIdAsync(resolvedCategoryId.Value) is null)
            resolvedCategoryId = null;

        var entry = new JournalEntry
        {
            Date = input.Date ?? DateOnly.FromDateTime(DateTime.Today),
            JournalTypeId = input.JournalTypeId,
            JournalCategoryId = resolvedCategoryId,
            Title = input.Title.Trim(),
            Body = input.Body.Trim(),
            LinkedTimeEntryId = input.LinkedTimeEntryId,
            CreatedAt = DateTime.UtcNow
        };
        return await journalRepo.AddAsync(entry);
    }
}
