using Microsoft.Playwright;
using TimeTracker.UITests.Infrastructure;

namespace TimeTracker.UITests.PageObjects;

/// <summary>
/// Page Object for the Tasks page (/tasks).
/// </summary>
public class TasksPage(IPage page) : PageObjectBase(page)
{
    protected override string Route => "/tasks";

    // ── Header ────────────────────────────────────────────────────────────────

    public ILocator AddTaskButton => Page.Locator("#add-task-btn");

    // ── Filter bar ────────────────────────────────────────────────────────────

    public ILocator FilterStatusSelect       => Page.Locator("#filter-status");
    public ILocator FilterPrioritySelect     => Page.Locator("#filter-priority");
    public ILocator FilterDeliverableToInput => Page.Locator("#filter-deliverable-to");
    public ILocator FilterCategorySelect     => Page.Locator("#filter-category");
    public ILocator ClearFiltersButton       => Page.Locator("button", new() { HasText = "Clear Filters" });

    // ── Task list ─────────────────────────────────────────────────────────────

    public ILocator EmptyState => Page.GetByText("No tasks found. Add your first task.");

    public ILocator TaskCards => Page.Locator("[data-testid='task-card']");

    public ILocator TaskCardByTitle(string title) =>
        Page.Locator("[data-testid='task-card']", new() { HasText = title });

    public ILocator EditButtonFor(string title) =>
        TaskCardByTitle(title).Locator("[title='Edit']");

    public ILocator DeleteButtonFor(string title) =>
        TaskCardByTitle(title).Locator("[title='Delete']");

    // ── Add / Edit modal ──────────────────────────────────────────────────────

    private ILocator ActiveModal => Page.Locator(".modal.show");

    public ILocator ModalHeading => ActiveModal.Locator(".modal-title");

    public ILocator TitleInput         => Page.Locator("#task-title");
    public ILocator DescriptionInput   => Page.Locator("#task-description");
    public ILocator StatusSelect       => Page.Locator("#task-status");
    public ILocator PrioritySelect     => Page.Locator("#task-priority");
    public ILocator DueDateInput       => Page.Locator("#task-due-date");
    public ILocator AssignedByInput    => Page.Locator("#task-assigned-by");
    public ILocator DeliverableToInput => Page.Locator("#task-deliverable-to");
    public ILocator CategorySelect     => Page.Locator("#task-category");
    public ILocator NotesInput         => Page.Locator("#task-notes");
    public ILocator SaveButton         => ActiveModal.Locator("button[type='submit']");
    public ILocator CancelButton       => ActiveModal.Locator("button.btn-secondary");

    // ── Delete confirm modal ──────────────────────────────────────────────────

    public ILocator DeleteConfirmButton => Page.Locator("#delete-confirm-btn");
    public ILocator DeleteCancelButton  => ActiveModal.Locator("button.btn-secondary");

    // ── Helpers ───────────────────────────────────────────────────────────────

    public async Task OpenAddFormAsync()
    {
        await AddTaskButton.ClickAsync();
        await ActiveModal.WaitForAsync();
    }

    public async Task FillAndSaveTaskAsync(
        string title,
        string? description = null,
        string? status = null,
        string? priority = null,
        string? assignedBy = null,
        string? deliverableTo = null,
        string? notes = null)
    {
        await TitleInput.FillAsync(title);

        if (description is not null)
            await DescriptionInput.FillAsync(description);

        if (status is not null)
            await StatusSelect.SelectOptionAsync(new SelectOptionValue { Label = status });

        if (priority is not null)
            await PrioritySelect.SelectOptionAsync(new SelectOptionValue { Label = priority });

        if (assignedBy is not null)
            await AssignedByInput.FillAsync(assignedBy);

        if (deliverableTo is not null)
            await DeliverableToInput.FillAsync(deliverableTo);

        if (notes is not null)
            await NotesInput.FillAsync(notes);

        await SaveButton.ClickAsync();
        await WaitForBlazorAsync();
    }
}
