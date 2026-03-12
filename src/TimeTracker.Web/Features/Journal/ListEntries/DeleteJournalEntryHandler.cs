using TimeTracker.Web.Data.Repositories;

namespace TimeTracker.Web.Features.Journal.ListEntries;

public class DeleteJournalEntryHandler(IJournalEntryRepository journalRepo)
{
    public Task HandleAsync(int id) => journalRepo.DeleteAsync(id);
}
