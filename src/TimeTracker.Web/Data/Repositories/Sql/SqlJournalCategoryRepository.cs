using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data.Models;

namespace TimeTracker.Web.Data.Repositories.Sql;

public class SqlJournalCategoryRepository(AppDbContext db) : IJournalCategoryRepository
{
    public async Task<JournalCategory?> GetByIdAsync(int id)
        => await db.JournalCategories.FindAsync(id);

    public async Task<List<JournalCategory>> GetAllAsync()
        => await db.JournalCategories.OrderBy(c => c.Id).ToListAsync();

    public async Task<JournalCategory> AddAsync(JournalCategory category)
    {
        db.JournalCategories.Add(category);
        await db.SaveChangesAsync();
        return category;
    }

    public async Task UpdateAsync(JournalCategory category)
    {
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var category = await db.JournalCategories.FindAsync(id);
        if (category is null) return;
        db.JournalCategories.Remove(category);
        await db.SaveChangesAsync();
    }
}
