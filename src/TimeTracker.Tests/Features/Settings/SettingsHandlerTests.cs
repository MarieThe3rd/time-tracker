using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data;
using TimeTracker.Web.Data.Models;
using TimeTracker.Web.Data.Repositories.Sql;
using TimeTracker.Web.Features.Settings;

namespace TimeTracker.Tests.Features.Settings;

public class SettingsHandlerTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static SettingsHandler CreateHandler(AppDbContext db) =>
        new SettingsHandler(new SqlUserSettingsRepository(db), new SqlWorkCategoryRepository(db));

    [Fact]
    public async Task GetAsync_WhenNoSettings_ReturnsDefault()
    {
        using var db = CreateDb();
        var handler = CreateHandler(db);

        var settings = await handler.GetAsync();

        Assert.NotNull(settings);
        Assert.Equal(@"Journal\Daily", settings.DailyNotesSubfolder);
    }

    [Fact]
    public async Task SaveAsync_PersistsVaultPath()
    {
        using var db = CreateDb();
        var handler = CreateHandler(db);
        var settings = await handler.GetAsync();
        settings.VaultRootPath = @"C:\MyVault";

        await handler.SaveAsync(settings);
        var reloaded = await handler.GetAsync();

        Assert.Equal(@"C:\MyVault", reloaded.VaultRootPath);
    }

    [Fact]
    public async Task AddCategoryAsync_AddsNewCategory()
    {
        using var db = CreateDb();
        var handler = CreateHandler(db);

        var cat = await handler.AddCategoryAsync("On Call", "#dc3545", "bi-telephone");

        Assert.Equal("On Call", cat.Name);
        Assert.Equal(1, await db.WorkCategories.CountAsync());
    }

    [Fact]
    public async Task DeleteCategoryAsync_SystemCategory_NotDeleted()
    {
        using var db = CreateDb();
        db.WorkCategories.Add(new WorkCategory
        {
            Id = 50, Name = "System Cat", Color = "#000", Icon = "bi-x", IsSystem = true
        });
        await db.SaveChangesAsync();

        var handler = CreateHandler(db);
        await handler.DeleteCategoryAsync(50);

        Assert.Equal(1, await db.WorkCategories.CountAsync());
    }

    [Fact]
    public async Task DeleteCategoryAsync_UserCategory_Deleted()
    {
        using var db = CreateDb();
        db.WorkCategories.Add(new WorkCategory
        {
            Id = 51, Name = "Custom", Color = "#000", Icon = "bi-x", IsSystem = false
        });
        await db.SaveChangesAsync();

        var handler = CreateHandler(db);
        await handler.DeleteCategoryAsync(51);

        Assert.Equal(0, await db.WorkCategories.CountAsync());
    }

    [Fact]
    public async Task UpdateCategoryAsync_ChangesName()
    {
        using var db = CreateDb();
        db.WorkCategories.Add(new WorkCategory
        {
            Id = 52, Name = "Old Name", Color = "#000", Icon = "bi-x", IsSystem = false
        });
        await db.SaveChangesAsync();

        var handler = CreateHandler(db);
        await handler.UpdateCategoryAsync(new WorkCategory
        {
            Id = 52, Name = "New Name", Color = "#fff", Icon = "bi-check", IsSystem = false
        });

        var cat = await db.WorkCategories.FindAsync(52);
        Assert.Equal("New Name", cat!.Name);
    }

    [Fact]
    public async Task GetAsync_WhenSettingsExist_ReturnsExistingSettings()
    {
        using var db = CreateDb();
        db.UserSettings.Add(new UserSettings { Id = 1, VaultRootPath = @"C:\ExistingVault", DailyNotesSubfolder = @"Notes\Daily" });
        await db.SaveChangesAsync();

        var handler = CreateHandler(db);
        var settings = await handler.GetAsync();

        Assert.Equal(@"C:\ExistingVault", settings.VaultRootPath);
        Assert.Equal(@"Notes\Daily", settings.DailyNotesSubfolder);
    }

    [Fact]
    public async Task SaveAsync_PersistsDailyNotesSubfolder()
    {
        using var db = CreateDb();
        var handler = CreateHandler(db);
        var settings = await handler.GetAsync();
        settings.DailyNotesSubfolder = @"MyNotes\Daily";

        await handler.SaveAsync(settings);
        var reloaded = await handler.GetAsync();

        Assert.Equal(@"MyNotes\Daily", reloaded.DailyNotesSubfolder);
    }

    [Fact]
    public async Task SaveAsync_NewSettings_AddsToDatabase()
    {
        using var db = CreateDb();
        var handler = CreateHandler(db);
        var settings = new UserSettings { Id = 1, VaultRootPath = @"C:\NewVault" };

        await handler.SaveAsync(settings);

        Assert.Equal(1, await db.UserSettings.CountAsync());
    }

    [Fact]
    public async Task GetCategoriesAsync_ReturnsAllCategories_OrderedByName()
    {
        using var db = CreateDb();
        db.WorkCategories.AddRange(
            new WorkCategory { Id = 1, Name = "Zeta", Color = "#000", Icon = "bi-z", IsSystem = false },
            new WorkCategory { Id = 2, Name = "Alpha", Color = "#000", Icon = "bi-a", IsSystem = false },
            new WorkCategory { Id = 3, Name = "Mu", Color = "#000", Icon = "bi-m", IsSystem = false }
        );
        await db.SaveChangesAsync();

        var handler = CreateHandler(db);
        var cats = await handler.GetCategoriesAsync();

        Assert.Equal(3, cats.Count);
        Assert.Equal("Alpha", cats[0].Name);
        Assert.Equal("Mu", cats[1].Name);
        Assert.Equal("Zeta", cats[2].Name);
    }

    [Fact]
    public async Task AddCategoryAsync_EmptyName_ThrowsArgumentException()
    {
        using var db = CreateDb();
        var handler = CreateHandler(db);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            handler.AddCategoryAsync("", "#000", "bi-x"));
    }

    [Fact]
    public async Task AddCategoryAsync_WhitespaceName_ThrowsArgumentException()
    {
        using var db = CreateDb();
        var handler = CreateHandler(db);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            handler.AddCategoryAsync("   ", "#000", "bi-x"));
    }

    [Fact]
    public async Task AddCategoryAsync_TrimsNameWhitespace()
    {
        using var db = CreateDb();
        var handler = CreateHandler(db);

        var cat = await handler.AddCategoryAsync("  Meetings  ", "#000", "bi-calendar");

        Assert.Equal("Meetings", cat.Name);
    }

    [Fact]
    public async Task UpdateCategoryAsync_EmptyName_ThrowsArgumentException()
    {
        using var db = CreateDb();
        db.WorkCategories.Add(new WorkCategory { Id = 60, Name = "Valid", Color = "#000", Icon = "bi-x", IsSystem = false });
        await db.SaveChangesAsync();

        var handler = CreateHandler(db);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            handler.UpdateCategoryAsync(new WorkCategory { Id = 60, Name = "", Color = "#000", Icon = "bi-x" }));
    }

    [Fact]
    public async Task UpdateCategoryAsync_NonExistentId_DoesNotThrow()
    {
        using var db = CreateDb();
        var handler = CreateHandler(db);

        var ex = await Record.ExceptionAsync(() =>
            handler.UpdateCategoryAsync(new WorkCategory { Id = 999, Name = "Ghost", Color = "#000", Icon = "bi-x" }));

        Assert.Null(ex);
    }

    [Fact]
    public async Task DeleteCategoryAsync_NonExistentId_DoesNotThrow()
    {
        using var db = CreateDb();
        var handler = CreateHandler(db);

        var ex = await Record.ExceptionAsync(() => handler.DeleteCategoryAsync(999));

        Assert.Null(ex);
    }
}