using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data;
using TimeTracker.Web.Data.Models;
using TimeTracker.Web.Data.Repositories.Sql;

namespace TimeTracker.Tests.Features.Journal;

public class SqlJournalTypeRepositoryTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new AppDbContext(options);
        db.Database.EnsureCreated(); // applies seed data
        return db;
    }

    private static SqlJournalTypeRepository CreateRepo(AppDbContext db) =>
        new SqlJournalTypeRepository(db);

    [Fact]
    public async Task GetAllAsync_ReturnsSeededSystemTypes()
    {
        using var db = CreateDb();
        var repo = CreateRepo(db);

        var types = await repo.GetAllAsync();

        Assert.Equal(3, types.Count);
        Assert.Contains(types, t => t.Name == "Challenge" && t.Id == 1);
        Assert.Contains(types, t => t.Name == "Learning" && t.Id == 2);
        Assert.Contains(types, t => t.Name == "Success" && t.Id == 3);
    }

    [Fact]
    public async Task GetAllAsync_SystemTypesHaveCorrectProperties()
    {
        using var db = CreateDb();
        var repo = CreateRepo(db);

        var types = await repo.GetAllAsync();
        var challenge = types.Single(t => t.Id == 1);
        var learning = types.Single(t => t.Id == 2);
        var success = types.Single(t => t.Id == 3);

        Assert.Equal("#ffc107", challenge.Color);
        Assert.Equal("bi-lightning-charge", challenge.Icon);
        Assert.True(challenge.IsSystem);

        Assert.Equal("#0dcaf0", learning.Color);
        Assert.Equal("bi-mortarboard", learning.Icon);
        Assert.True(learning.IsSystem);

        Assert.Equal("#198754", success.Color);
        Assert.Equal("bi-trophy", success.Icon);
        Assert.True(success.IsSystem);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsType()
    {
        using var db = CreateDb();
        var repo = CreateRepo(db);

        var type = await repo.GetByIdAsync(1);

        Assert.NotNull(type);
        Assert.Equal("Challenge", type.Name);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentId_ReturnsNull()
    {
        using var db = CreateDb();
        var repo = CreateRepo(db);

        var type = await repo.GetByIdAsync(999);

        Assert.Null(type);
    }

    [Fact]
    public async Task AddAsync_NonSystemType_AddsSuccessfully()
    {
        using var db = CreateDb();
        var repo = CreateRepo(db);

        var newType = new JournalType { Name = "Custom", Color = "#ff0000", Icon = "bi-star", IsSystem = false };
        var added = await repo.AddAsync(newType);

        Assert.True(added.Id > 0);
        Assert.Equal("Custom", added.Name);
        Assert.Equal(4, await db.JournalTypes.CountAsync()); // 3 seed + 1 new
    }

    [Fact]
    public async Task UpdateAsync_ModifiesType()
    {
        using var db = CreateDb();
        var repo = CreateRepo(db);

        var newType = new JournalType { Name = "Before", Color = "#aaaaaa", Icon = "bi-circle", IsSystem = false };
        await repo.AddAsync(newType);
        newType.Name = "After";
        newType.Color = "#bbbbbb";
        await repo.UpdateAsync(newType);

        var fromDb = await db.JournalTypes.FindAsync(newType.Id);
        Assert.Equal("After", fromDb!.Name);
        Assert.Equal("#bbbbbb", fromDb.Color);
    }

    [Fact]
    public async Task DeleteAsync_NonSystemType_DeletesSuccessfully()
    {
        using var db = CreateDb();
        var repo = CreateRepo(db);

        var newType = new JournalType { Name = "Deletable", Color = "#cccccc", Icon = "bi-x", IsSystem = false };
        await repo.AddAsync(newType);
        var countBefore = await db.JournalTypes.CountAsync();

        await repo.DeleteAsync(newType.Id);

        Assert.Equal(countBefore - 1, await db.JournalTypes.CountAsync());
    }

    [Fact]
    public async Task DeleteAsync_SystemType_ThrowsInvalidOperationException()
    {
        using var db = CreateDb();
        var repo = CreateRepo(db);

        await Assert.ThrowsAsync<InvalidOperationException>(() => repo.DeleteAsync(1));
        await Assert.ThrowsAsync<InvalidOperationException>(() => repo.DeleteAsync(2));
        await Assert.ThrowsAsync<InvalidOperationException>(() => repo.DeleteAsync(3));
    }

    [Fact]
    public async Task DeleteAsync_NonExistentId_DoesNotThrow()
    {
        using var db = CreateDb();
        var repo = CreateRepo(db);

        var ex = await Record.ExceptionAsync(() => repo.DeleteAsync(999));
        Assert.Null(ex);
    }
}
