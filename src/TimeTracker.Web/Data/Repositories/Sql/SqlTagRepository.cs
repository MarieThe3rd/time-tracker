using Microsoft.EntityFrameworkCore;
using TimeTracker.Web.Data.Models;

namespace TimeTracker.Web.Data.Repositories.Sql;

public class SqlTagRepository(AppDbContext db) : ITagRepository
{
    public async Task<Tag?> GetByIdAsync(int id)
        => await db.Tags.FindAsync(id);

    public async Task<Tag?> GetByNameAsync(string name)
        => await db.Tags.FirstOrDefaultAsync(t => t.Name == name);

    public async Task<List<Tag>> GetAllAsync()
        => await db.Tags.OrderBy(t => t.Name).ToListAsync();

    public async Task<Tag> AddAsync(Tag tag)
    {
        db.Tags.Add(tag);
        await db.SaveChangesAsync();
        return tag;
    }

    public async Task DeleteAsync(int id)
    {
        var tag = await db.Tags.FindAsync(id);
        if (tag is not null)
        {
            db.Tags.Remove(tag);
            await db.SaveChangesAsync();
        }
    }

    public async Task AddTimeEntryTagAsync(int timeEntryId, int tagId)
    {
        var link = new TimeEntryTag { TimeEntryId = timeEntryId, TagId = tagId };
        db.TimeEntryTags.Add(link);
        await db.SaveChangesAsync();
    }

    public async Task RemoveTimeEntryTagAsync(int timeEntryId, int tagId)
    {
        var link = await db.TimeEntryTags.FindAsync(timeEntryId, tagId);
        if (link is not null)
        {
            db.TimeEntryTags.Remove(link);
            await db.SaveChangesAsync();
        }
    }

    public async Task<List<Tag>> GetTagsForTimeEntryAsync(int timeEntryId)
        => await db.TimeEntryTags
            .Where(t => t.TimeEntryId == timeEntryId)
            .Select(t => t.Tag)
            .ToListAsync();
}
