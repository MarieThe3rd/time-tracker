using TimeTracker.Web.Data.Models;
using TimeTracker.Web.Data.Repositories;

namespace TimeTracker.Web.Features.Journal.ListEntries;

public record JournalFilter(
    int? JournalTypeId = null,
    DateOnly? From = null,
    DateOnly? To = null,
    int? CategoryId = null);

public class ListEntriesHandler(IJournalEntryRepository journalRepo)
{
    public async Task<List<JournalEntry>> HandleAsync(JournalFilter filter)
    {
        if (filter.From.HasValue && filter.To.HasValue && filter.From.Value > filter.To.Value)
            throw new ArgumentException("From date must be on or before To date.");

        return await journalRepo.GetFilteredAsync(filter.JournalTypeId, filter.From, filter.To, includeLinkedTimeEntry: true, filter.CategoryId);
    }
}
