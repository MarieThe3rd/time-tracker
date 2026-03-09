namespace TimeTracker.Web.Features.Timer;

public class RunningTimerService
{
    private readonly object _lock = new();

    private bool _isRunning;
    private DateTime? _startedAt;
    private int? _categoryId;
    private string? _description;

    public bool IsRunning { get { lock (_lock) return _isRunning; } }
    public DateTime? StartedAt { get { lock (_lock) return _startedAt; } }
    public int? CategoryId { get { lock (_lock) return _categoryId; } }
    public string? Description { get { lock (_lock) return _description; } }

    public void Start(int? categoryId, string? description)
    {
        lock (_lock)
        {
            _isRunning = true;
            _startedAt = DateTime.UtcNow;
            _categoryId = categoryId;
            _description = description;
        }
    }

    public TimeSpan GetElapsed()
    {
        lock (_lock)
        {
            return _isRunning && _startedAt.HasValue
                ? DateTime.UtcNow - _startedAt.Value
                : TimeSpan.Zero;
        }
    }

    public (DateTime startedAt, int? categoryId, string? description) Stop()
    {
        lock (_lock)
        {
            var snapshot = (
                startedAt: _startedAt ?? DateTime.UtcNow,
                categoryId: _categoryId,
                description: _description
            );
            _isRunning = false;
            _startedAt = null;
            _categoryId = null;
            _description = null;
            return snapshot;
        }
    }
}
