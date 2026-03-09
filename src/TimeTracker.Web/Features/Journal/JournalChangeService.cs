namespace TimeTracker.Web.Features.Journal;

/// <summary>
/// Scoped service (one per Blazor circuit) that allows QuickAddPanel to signal
/// JournalPage to refresh its list after a new entry is saved.
/// </summary>
public class JournalChangeService
{
    public event Action? EntryAdded;
    public void NotifyEntryAdded() => EntryAdded?.Invoke();
}
