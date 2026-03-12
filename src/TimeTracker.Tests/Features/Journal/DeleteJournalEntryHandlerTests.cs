using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data;
using TimeTracker.Web.Data.Models;
using TimeTracker.Web.Data.Repositories.Sql;
using TimeTracker.Web.Features.Journal.ListEntries;

namespace TimeTracker.Tests.Features.Journal;

public class DeleteJournalEntryHandlerTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task HandleAsync_ExistingEntry_DeletesIt()
    {
        using var db = CreateDb();
        db.JournalEntries.Add(new JournalEntry
        {
            Id = 1,
            Date = DateOnly.FromDateTime(DateTime.Today),
            Type = JournalEntryType.Success,
            Title = "A win",
            Body = "",
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var handler = new DeleteJournalEntryHandler(new SqlJournalEntryRepository(db));
        await handler.HandleAsync(1);

        Assert.Equal(0, await db.JournalEntries.CountAsync());
    }

    [Fact]
    public async Task HandleAsync_EntryIsActuallyRemoved_FromDatabase()
    {
        using var db = CreateDb();
        db.JournalEntries.Add(new JournalEntry
        {
            Id = 10,
            Date = DateOnly.FromDateTime(DateTime.Today),
            Type = JournalEntryType.Learning,
            Title = "Learned something",
            Body = "Details here",
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var handler = new DeleteJournalEntryHandler(new SqlJournalEntryRepository(db));
        await handler.HandleAsync(10);

        var found = await db.JournalEntries.FindAsync(10);
        Assert.Null(found);
    }

    [Fact]
    public async Task HandleAsync_NonExistentId_DoesNotThrow()
    {
        using var db = CreateDb();
        var handler = new DeleteJournalEntryHandler(new SqlJournalEntryRepository(db));

        var ex = await Record.ExceptionAsync(() => handler.HandleAsync(999));

        Assert.Null(ex);
    }

    [Fact]
    public async Task HandleAsync_OnlyDeletesTargetEntry_LeavesOthersUntouched()
    {
        using var db = CreateDb();
        db.JournalEntries.AddRange(
            new JournalEntry { Id = 20, Date = DateOnly.FromDateTime(DateTime.Today), Type = JournalEntryType.Success, Title = "Keep me", Body = "", CreatedAt = DateTime.UtcNow },
            new JournalEntry { Id = 21, Date = DateOnly.FromDateTime(DateTime.Today), Type = JournalEntryType.Challenge, Title = "Delete me", Body = "", CreatedAt = DateTime.UtcNow }
        );
        await db.SaveChangesAsync();

        var handler = new DeleteJournalEntryHandler(new SqlJournalEntryRepository(db));
        await handler.HandleAsync(21);

        Assert.Equal(1, await db.JournalEntries.CountAsync());
        Assert.NotNull(await db.JournalEntries.FindAsync(20));
    }
}