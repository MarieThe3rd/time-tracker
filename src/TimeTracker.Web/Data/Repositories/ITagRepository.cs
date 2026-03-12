using TimeTracker.Web.Data.Models;

namespace TimeTracker.Web.Data.Repositories;

public interface ITagRepository
{
    Task<Tag?> GetByIdAsync(int id);
    Task<Tag?> GetByNameAsync(string name);
    Task<List<Tag>> GetAllAsync();
    Task<Tag> AddAsync(Tag tag);
    Task DeleteAsync(int id);
    Task AddTimeEntryTagAsync(int timeEntryId, int tagId);
    Task RemoveTimeEntryTagAsync(int timeEntryId, int tagId);
    Task<List<Tag>> GetTagsForTimeEntryAsync(int timeEntryId);
}
