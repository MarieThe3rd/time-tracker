using TimeTracker.Web.Features.Timer;

namespace TimeTracker.Tests.Features.Timer;

public class RunningTimerServiceTests
{
    [Fact]
    public void Start_SetsIsRunningTrue()
    {
        var svc = new RunningTimerService();

        svc.Start(null, null);

        Assert.True(svc.IsRunning);
    }

    [Fact]
    public void Start_StoresCategory()
    {
        var svc = new RunningTimerService();

        svc.Start(42, "Working on feature");

        Assert.Equal(42, svc.CategoryId);
        Assert.Equal("Working on feature", svc.Description);
    }

    [Fact]
    public void Start_WhenAlreadyRunning_OverwritesPreviousTimer()
    {
        var svc = new RunningTimerService();
        svc.Start(1, "First task");

        svc.Start(2, "Second task");

        Assert.True(svc.IsRunning);
        Assert.Equal(2, svc.CategoryId);
        Assert.Equal("Second task", svc.Description);
    }

    [Fact]
    public void Stop_SetsIsRunningFalse()
    {
        var svc = new RunningTimerService();
        svc.Start(1, "Task");

        svc.Stop();

        Assert.False(svc.IsRunning);
    }

    [Fact]
    public void Stop_ReturnsSnapshot_WithStoredValues()
    {
        var svc = new RunningTimerService();
        svc.Start(7, "Sprint review");

        var (_, catId, desc) = svc.Stop();

        Assert.Equal(7, catId);
        Assert.Equal("Sprint review", desc);
    }

    [Fact]
    public void Stop_ClearsStateAfterCall()
    {
        var svc = new RunningTimerService();
        svc.Start(1, "Task");
        svc.Stop();

        // Second stop should return null values
        var (_, catId, desc) = svc.Stop();

        Assert.Null(catId);
        Assert.Null(desc);
        Assert.False(svc.IsRunning);
    }

    [Fact]
    public void Stop_WhenNeverStarted_ReturnsNullCategoryAndDescription()
    {
        var svc = new RunningTimerService();

        var (startedAt, catId, desc) = svc.Stop();

        Assert.Null(catId);
        Assert.Null(desc);
        Assert.True(startedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void GetElapsed_WhenNotRunning_ReturnsZero()
    {
        var svc = new RunningTimerService();

        var elapsed = svc.GetElapsed();

        Assert.Equal(TimeSpan.Zero, elapsed);
    }

    [Fact]
    public async Task GetElapsed_WhenRunning_ReturnsPositiveTimeSpan()
    {
        var svc = new RunningTimerService();
        svc.Start(null, null);
        await Task.Delay(50);

        var elapsed = svc.GetElapsed();

        Assert.True(elapsed > TimeSpan.Zero);
    }

    [Fact]
    public void GetElapsed_AfterStop_ReturnsZero()
    {
        var svc = new RunningTimerService();
        svc.Start(null, null);
        svc.Stop();

        var elapsed = svc.GetElapsed();

        Assert.Equal(TimeSpan.Zero, elapsed);
    }

    [Fact]
    public void IsRunning_DefaultsToFalse()
    {
        var svc = new RunningTimerService();

        Assert.False(svc.IsRunning);
    }
}
