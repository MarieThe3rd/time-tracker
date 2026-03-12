using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data;
using TimeTracker.Web.Data.Models;
using TimeTracker.Web.Data.Repositories.Sql;
using TimeTracker.Web.Features.Journal.AddEntry;

namespace TimeTracker.Tests.Features.Journal;

public class AddEntryHandlerCategoryValidationTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static AddEntryHandler CreateHandler(AppDbContext db) =>
        new AddEntryHandler(new SqlJournalEntryRepository(db), new SqlJournalCategoryRepository(db));

    [Fact]
    public async Task Add_with_valid_category_id_saves_category()
    {
        using var db = CreateDb();
        db.JournalCategories.Add(new JournalCategory { Id = 3, Name = "Work", Color = "#fff", Icon = "bi-briefcase" });
        await db.SaveChangesAsync();

        var handler = CreateHandler(db);
        var result = await handler.HandleAsync(
            new AddJournalEntryInput(JournalTypeId: 1, "Good day", "", JournalCategoryId: 3));

        Assert.Equal(3, result.JournalCategoryId);
    }

    [Fact]
    public async Task Add_with_invalid_category_id_nulls_out_category()
    {
        using var db = CreateDb();
        var handler = CreateHandler(db);

        var result = await handler.HandleAsync(
            new AddJournalEntryInput(JournalTypeId: 1, "Good day", "", JournalCategoryId: 999));

        Assert.Null(result.JournalCategoryId);
        var fromDb = await db.JournalEntries.FindAsync(result.Id);
        Assert.Null(fromDb!.JournalCategoryId);
    }

    [Fact]
    public async Task Add_with_null_category_id_saves_null()
    {
        using var db = CreateDb();
        var handler = CreateHandler(db);

        var result = await handler.HandleAsync(
            new AddJournalEntryInput(JournalTypeId: 1, "Good day", ""));

        Assert.Null(result.JournalCategoryId);
    }
}
