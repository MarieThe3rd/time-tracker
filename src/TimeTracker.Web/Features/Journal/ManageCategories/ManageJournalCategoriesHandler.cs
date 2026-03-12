using TimeTracker.Web.Data.Models;
using TimeTracker.Web.Data.Repositories;

namespace TimeTracker.Web.Features.Journal.ManageCategories;

public class ManageJournalCategoriesHandler(IJournalCategoryRepository categoryRepo)
{
    public Task<List<JournalCategory>> GetAllAsync() => categoryRepo.GetAllAsync();

    public async Task<JournalCategory> AddAsync(string name, string color, string icon)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name cannot be empty.", nameof(name));

        var cat = new JournalCategory { Name = name.Trim(), Color = color, Icon = icon };
        return await categoryRepo.AddAsync(cat);
    }

    public async Task UpdateAsync(JournalCategory category)
    {
        if (string.IsNullOrWhiteSpace(category.Name))
            throw new ArgumentException("Category name cannot be empty.", nameof(category));

        var existing = await categoryRepo.GetByIdAsync(category.Id);
        if (existing is null) return;
        existing.Name = category.Name.Trim();
        existing.Color = category.Color;
        existing.Icon = category.Icon;
        await categoryRepo.UpdateAsync(existing);
    }

    public Task DeleteAsync(int id) => categoryRepo.DeleteAsync(id);
}
