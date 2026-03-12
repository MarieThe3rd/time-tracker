using TimeTracker.UITests.Infrastructure;
using TimeTracker.UITests.PageObjects;

namespace TimeTracker.UITests.Tests;

[Collection("App")]
public class TasksTests(AppFixture app)
{
    // ── Page load ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task TasksPage_PageLoads_ShowsHeading()
    {
        var page = await app.NewPageAsync();
        var tasksPage = new TasksPage(page);
        await tasksPage.GotoAsync();

        var heading = await tasksPage.GetHeadingAsync();

        Assert.Contains("Tasks", heading);
    }

    [Fact]
    public async Task TasksPage_PageLoads_ShowsAddTaskButton()
    {
        var page = await app.NewPageAsync();
        var tasksPage = new TasksPage(page);
        await tasksPage.GotoAsync();

        await tasksPage.AddTaskButton.WaitForAsync();

        Assert.True(await tasksPage.AddTaskButton.IsVisibleAsync());
    }

    [Fact]
    public async Task TasksPage_PageLoads_ShowsFilterBar()
    {
        var page = await app.NewPageAsync();
        var tasksPage = new TasksPage(page);
        await tasksPage.GotoAsync();

        Assert.True(await tasksPage.FilterStatusSelect.IsVisibleAsync());
        Assert.True(await tasksPage.FilterPrioritySelect.IsVisibleAsync());
        Assert.True(await tasksPage.FilterDeliverableToInput.IsVisibleAsync());
        Assert.True(await tasksPage.FilterCategorySelect.IsVisibleAsync());
    }

    /// <summary>
    /// Verifies the empty-state message when no tasks exist.
    /// NOTE: This test assumes a clean UITest database. If tasks have been added by
    /// previous test runs the empty-state element will not be visible; consider resetting
    /// the database or skipping this test in subsequent runs.
    /// </summary>
    [Fact]
    public async Task TasksPage_WhenNoTasks_ShowsEmptyState()
    {
        var page = await app.NewPageAsync();
        var tasksPage = new TasksPage(page);
        await tasksPage.GotoAsync();

        // Only assert empty state when the list truly is empty
        var cardCount = await tasksPage.TaskCards.CountAsync();
        if (cardCount == 0)
        {
            await tasksPage.EmptyState.WaitForAsync();
            Assert.True(await tasksPage.EmptyState.IsVisibleAsync());
        }
        else
        {
            // If tasks already exist, verify the empty state is NOT shown (correct behaviour)
            Assert.False(await tasksPage.EmptyState.IsVisibleAsync(),
                "Empty-state message should not be visible when tasks are present.");
        }
    }

    // ── Add task ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task TasksPage_AddTask_ModalOpens_WithNewTaskTitle()
    {
        var page = await app.NewPageAsync();
        var tasksPage = new TasksPage(page);
        await tasksPage.GotoAsync();

        await tasksPage.OpenAddFormAsync();

        var heading = await tasksPage.ModalHeading.InnerTextAsync();
        Assert.Contains("New Task", heading);
    }

    [Fact]
    public async Task TasksPage_AddTask_ModalCancels_AndModalDisappears()
    {
        var page = await app.NewPageAsync();
        var tasksPage = new TasksPage(page);
        await tasksPage.GotoAsync();

        await tasksPage.OpenAddFormAsync();
        await tasksPage.CancelButton.ClickAsync();
        await tasksPage.WaitForBlazorAsync();

        Assert.Equal(0, await page.Locator(".modal.show").CountAsync());
    }

    [Fact]
    public async Task TasksPage_AddTask_HappyPath_TaskAppearsInList()
    {
        var page = await app.NewPageAsync();
        var tasksPage = new TasksPage(page);
        await tasksPage.GotoAsync();

        const string title = "UI Test — Add Task Happy Path";
        await tasksPage.OpenAddFormAsync();
        await tasksPage.FillAndSaveTaskAsync(
            title: title,
            description: "Created by Playwright",
            priority: "High",
            deliverableTo: "QA Team",
            notes: "Automated test note");

        await tasksPage.TaskCardByTitle(title).WaitForAsync();
        Assert.True(await tasksPage.TaskCardByTitle(title).IsVisibleAsync());
    }

    [Fact]
    public async Task TasksPage_AddTask_MissingTitle_ShowsValidationError()
    {
        var page = await app.NewPageAsync();
        var tasksPage = new TasksPage(page);
        await tasksPage.GotoAsync();

        await tasksPage.OpenAddFormAsync();
        // Submit without filling the required Title field
        await tasksPage.SaveButton.ClickAsync();
        await tasksPage.WaitForBlazorAsync();

        // Modal should remain open and show the validation message
        Assert.True(await page.Locator("text=Title is required.").IsVisibleAsync(),
            "Expected 'Title is required.' validation message.");
        Assert.Equal(1, await page.Locator(".modal.show").CountAsync());
    }

    // ── Edit task ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task TasksPage_EditTask_ModalOpens_WithEditTaskTitle()
    {
        var page = await app.NewPageAsync();
        var tasksPage = new TasksPage(page);
        await tasksPage.GotoAsync();

        // Add a task to edit
        const string title = "UI Test — Edit Task (setup)";
        await tasksPage.OpenAddFormAsync();
        await tasksPage.FillAndSaveTaskAsync(title: title);

        // Open edit form
        await tasksPage.EditButtonFor(title).ClickAsync();
        await tasksPage.WaitForBlazorAsync();

        var heading = await tasksPage.ModalHeading.InnerTextAsync();
        Assert.Contains("Edit Task", heading);
    }

    [Fact]
    public async Task TasksPage_EditTask_UpdatesTitle_AndReflectsInList()
    {
        var page = await app.NewPageAsync();
        var tasksPage = new TasksPage(page);
        await tasksPage.GotoAsync();

        const string original = "UI Test — Edit Task Original";
        const string updated  = "UI Test — Edit Task Updated";

        // Add original
        await tasksPage.OpenAddFormAsync();
        await tasksPage.FillAndSaveTaskAsync(title: original);

        // Edit it
        await tasksPage.EditButtonFor(original).ClickAsync();
        await tasksPage.WaitForBlazorAsync();

        await tasksPage.TitleInput.ClearAsync();
        await tasksPage.TitleInput.FillAsync(updated);
        await tasksPage.SaveButton.ClickAsync();
        await tasksPage.WaitForBlazorAsync();

        // Updated title should be in list; original should be gone
        Assert.True(await tasksPage.TaskCardByTitle(updated).IsVisibleAsync(),
            "Expected updated title to appear in the task list.");
        Assert.Equal(0, await tasksPage.TaskCardByTitle(original).CountAsync());
    }

    [Fact]
    public async Task TasksPage_EditTask_FormIsPreFilledWithExistingValues()
    {
        var page = await app.NewPageAsync();
        var tasksPage = new TasksPage(page);
        await tasksPage.GotoAsync();

        const string title = "UI Test — Edit Task Prefill";
        const string description = "Prefill description";

        await tasksPage.OpenAddFormAsync();
        await tasksPage.FillAndSaveTaskAsync(title: title, description: description);

        await tasksPage.EditButtonFor(title).ClickAsync();
        await tasksPage.WaitForBlazorAsync();

        Assert.Equal(title,       await tasksPage.TitleInput.InputValueAsync());
        Assert.Equal(description, await tasksPage.DescriptionInput.InputValueAsync());
    }

    // ── Delete task ───────────────────────────────────────────────────────────

    [Fact]
    public async Task TasksPage_DeleteTask_ConfirmDialog_Opens()
    {
        var page = await app.NewPageAsync();
        var tasksPage = new TasksPage(page);
        await tasksPage.GotoAsync();

        const string title = "UI Test — Delete Task Dialog";
        await tasksPage.OpenAddFormAsync();
        await tasksPage.FillAndSaveTaskAsync(title: title);

        await tasksPage.DeleteButtonFor(title).ClickAsync();
        await tasksPage.WaitForBlazorAsync();

        Assert.True(await page.Locator(".modal.show").IsVisibleAsync(),
            "Expected delete confirmation modal to be visible.");
        Assert.True(await page.Locator("text=Delete Task").IsVisibleAsync());
    }

    [Fact]
    public async Task TasksPage_DeleteTask_CancelKeepsTask()
    {
        var page = await app.NewPageAsync();
        var tasksPage = new TasksPage(page);
        await tasksPage.GotoAsync();

        const string title = "UI Test — Delete Cancel";
        await tasksPage.OpenAddFormAsync();
        await tasksPage.FillAndSaveTaskAsync(title: title);

        await tasksPage.DeleteButtonFor(title).ClickAsync();
        await tasksPage.WaitForBlazorAsync();
        await tasksPage.DeleteCancelButton.ClickAsync();
        await tasksPage.WaitForBlazorAsync();

        Assert.True(await tasksPage.TaskCardByTitle(title).IsVisibleAsync(),
            "Expected task to remain after cancelling deletion.");
    }

    [Fact]
    public async Task TasksPage_DeleteTask_ConfirmRemovesTask()
    {
        var page = await app.NewPageAsync();
        var tasksPage = new TasksPage(page);
        await tasksPage.GotoAsync();

        const string title = "UI Test — Delete Task Confirm";
        await tasksPage.OpenAddFormAsync();
        await tasksPage.FillAndSaveTaskAsync(title: title);

        await tasksPage.DeleteButtonFor(title).ClickAsync();
        await tasksPage.WaitForBlazorAsync();
        await tasksPage.DeleteConfirmButton.ClickAsync();
        await tasksPage.WaitForBlazorAsync();

        Assert.Equal(0, await tasksPage.TaskCardByTitle(title).CountAsync());
    }

    // ── Filtering ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task TasksPage_FilterByStatus_HidesNonMatchingTasks()
    {
        var page = await app.NewPageAsync();
        var tasksPage = new TasksPage(page);
        await tasksPage.GotoAsync();

        // Add a "Done" task and a "Not Started" task
        const string doneTitle       = "UI Test — Filter Status Done";
        const string notStartedTitle = "UI Test — Filter Status Not Started";

        await tasksPage.OpenAddFormAsync();
        await tasksPage.FillAndSaveTaskAsync(title: doneTitle, status: "Done");

        await tasksPage.OpenAddFormAsync();
        await tasksPage.FillAndSaveTaskAsync(title: notStartedTitle);

        // Filter to "Done" only
        await tasksPage.FilterStatusSelect.SelectOptionAsync(new Microsoft.Playwright.SelectOptionValue { Label = "Done" });
        await tasksPage.WaitForBlazorAsync();

        Assert.True(await tasksPage.TaskCardByTitle(doneTitle).IsVisibleAsync(),
            "Expected Done task to be visible after filtering by Done status.");
        Assert.Equal(0, await tasksPage.TaskCardByTitle(notStartedTitle).CountAsync());
    }

    [Fact]
    public async Task TasksPage_ClearFilters_ShowsAllTasks()
    {
        var page = await app.NewPageAsync();
        var tasksPage = new TasksPage(page);
        await tasksPage.GotoAsync();

        // Add two tasks with different priorities
        const string criticalTitle = "UI Test — Filter Clear Critical";
        const string lowTitle      = "UI Test — Filter Clear Low";

        await tasksPage.OpenAddFormAsync();
        await tasksPage.FillAndSaveTaskAsync(title: criticalTitle, priority: "Critical");

        await tasksPage.OpenAddFormAsync();
        await tasksPage.FillAndSaveTaskAsync(title: lowTitle, priority: "Low");

        // Filter to Critical — Low disappears
        await tasksPage.FilterPrioritySelect.SelectOptionAsync(
            new Microsoft.Playwright.SelectOptionValue { Label = "Critical" });
        await tasksPage.WaitForBlazorAsync();

        // Clear filters — Low should come back
        await tasksPage.ClearFiltersButton.ClickAsync();
        await tasksPage.WaitForBlazorAsync();

        Assert.True(await tasksPage.TaskCardByTitle(criticalTitle).IsVisibleAsync());
        Assert.True(await tasksPage.TaskCardByTitle(lowTitle).IsVisibleAsync());
    }

    // ── Overdue badge ─────────────────────────────────────────────────────────

    [Fact]
    public async Task TasksPage_OverdueTask_ShowsWarningIndicator()
    {
        var page = await app.NewPageAsync();
        var tasksPage = new TasksPage(page);
        await tasksPage.GotoAsync();

        const string title = "UI Test — Overdue Task Warning";
        var yesterday = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));

        await tasksPage.OpenAddFormAsync();
        await tasksPage.TitleInput.FillAsync(title);
        // Set due date to yesterday so the task becomes overdue
        await tasksPage.DueDateInput.FillAsync(yesterday.ToString("yyyy-MM-dd"));
        await tasksPage.SaveButton.ClickAsync();
        await tasksPage.WaitForBlazorAsync();

        var card = tasksPage.TaskCardByTitle(title);
        await card.WaitForAsync();

        // The overdue indicator is the ⚠ symbol rendered inside the due-date small element
        var overdueIndicator = card.Locator("text=⚠");
        Assert.True(await overdueIndicator.IsVisibleAsync(),
            "Expected ⚠ overdue indicator on a task whose due date is in the past.");
    }
}
