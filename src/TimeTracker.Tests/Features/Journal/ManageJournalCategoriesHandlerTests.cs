using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data;
using TimeTracker.Web.Data.Models;
using TimeTracker.Web.Data.Repositories.Sql;
using TimeTracker.Web.Features.Journal.ManageCategories;

namespace TimeTracker.Tests.Features.Journal;

public class ManageJournalCategoriesHandlerTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static ManageJournalCategoriesHandler CreateHandler(AppDbContext db) =>
        new ManageJournalCategoriesHandler(new SqlJournalCategoryRepository(db));

    [Fact]
    public async Task GetAll_returns_all_categories()
    {
        using var db = CreateDb();
        db.JournalCategories.AddRange(
            new JournalCategory { Id = 1, Name = "Work", Color = "#fff", Icon = "bi-briefcase" },
            new JournalCategory { Id = 2, Name = "Personal", Color = "#000", Icon = "bi-person" }
        );
        await db.SaveChangesAsync();

        var handler = CreateHandler(db);
        var result = await handler.GetAllAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task Add_with_valid_name_creates_category()
    {
        using var db = CreateDb();
        var handler = CreateHandler(db);

        var result = await handler.AddAsync("  Focus  ", "#ff0000", "bi-lightning");

        Assert.Equal("Focus", result.Name);
        Assert.Equal("#ff0000", result.Color);
        Assert.Equal("bi-lightning", result.Icon);
        Assert.Equal(1, await db.JournalCategories.CountAsync());
    }

    [Fact]
    public async Task Add_with_empty_name_throws()
    {
        using var db = CreateDb();
        var handler = CreateHandler(db);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            handler.AddAsync("", "#000", "bi-x"));

        Assert.Equal(0, await db.JournalCategories.CountAsync());
    }

    [Fact]
    public async Task Add_with_whitespace_name_throws()
    {
        using var db = CreateDb();
        var handler = CreateHandler(db);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            handler.AddAsync("   ", "#000", "bi-x"));

        Assert.Equal(0, await db.JournalCategories.CountAsync());
    }

    [Fact]
    public async Task Update_with_valid_name_patches_and_saves()
    {
        using var db = CreateDb();
        db.JournalCategories.Add(new JournalCategory
        {
            Id = 10, Name = "Old Name", Color = "#000", Icon = "bi-tag"
        });
        await db.SaveChangesAsync();

        var handler = CreateHandler(db);
        await handler.UpdateAsync(new JournalCategory
        {
            Id = 10, Name = "  New Name  ", Color = "#abc", Icon = "bi-star"
        });

        var updated = await db.JournalCategories.FindAsync(10);
        Assert.Equal("New Name", updated!.Name);
        Assert.Equal("#abc", updated.Color);
        Assert.Equal("bi-star", updated.Icon);
    }

    [Fact]
    public async Task Update_with_empty_name_throws()
    {
        using var db = CreateDb();
        db.JournalCategories.Add(new JournalCategory
        {
            Id = 20, Name = "Valid", Color = "#000", Icon = "bi-tag"
        });
        await db.SaveChangesAsync();

        var handler = CreateHandler(db);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            handler.UpdateAsync(new JournalCategory { Id = 20, Name = "", Color = "#000", Icon = "bi-tag" }));
    }

    [Fact]
    public async Task Update_with_nonexistent_id_is_noop()
    {
        using var db = CreateDb();
        var handler = CreateHandler(db);

        var ex = await Record.ExceptionAsync(() =>
            handler.UpdateAsync(new JournalCategory { Id = 999, Name = "Ghost", Color = "#000", Icon = "bi-x" }));

        Assert.Null(ex);
    }

    [Fact]
    public async Task Delete_delegates_to_repository()
    {
        using var db = CreateDb();
        db.JournalCategories.Add(new JournalCategory
        {
            Id = 30, Name = "ToDelete", Color = "#000", Icon = "bi-trash"
        });
        await db.SaveChangesAsync();

        var handler = CreateHandler(db);
        await handler.DeleteAsync(30);

        Assert.Equal(0, await db.JournalCategories.CountAsync());
    }
}
