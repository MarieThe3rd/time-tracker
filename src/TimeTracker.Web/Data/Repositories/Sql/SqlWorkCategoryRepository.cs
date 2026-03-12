using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data.Models;

namespace TimeTracker.Web.Data.Repositories.Sql;

public class SqlWorkCategoryRepository(AppDbContext db) : IWorkCategoryRepository
{
    public async Task<WorkCategory?> GetByIdAsync(int id)
        => await db.WorkCategories.FindAsync(id);

    public async Task<List<WorkCategory>> GetAllAsync()
        => await db.WorkCategories.OrderBy(c => c.Name).ToListAsync();

    public async Task<WorkCategory> AddAsync(WorkCategory category)
    {
        db.WorkCategories.Add(category);
        await db.SaveChangesAsync();
        return category;
    }

    public async Task UpdateAsync(WorkCategory category)
    {
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var cat = await db.WorkCategories.FindAsync(id);
        if (cat is not null && !cat.IsSystem)
        {
            db.WorkCategories.Remove(cat);
            await db.SaveChangesAsync();
        }
    }
}
