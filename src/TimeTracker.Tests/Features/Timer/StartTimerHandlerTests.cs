using TimeTracker.Web.Features.Timer;

namespace TimeTracker.Tests.Features.Timer;

public class StartTimerHandlerTests
{
    [Fact]
    public void Handle_StartsTimer()
    {
        var svc = new RunningTimerService();
        var handler = new StartTimerHandler(svc);

        handler.Handle(null, null);

        Assert.True(svc.IsRunning);
    }

    [Fact]
    public void Handle_WithCategory_PropagatesCategoryId()
    {
        var svc = new RunningTimerService();
        var handler = new StartTimerHandler(svc);

        handler.Handle(5, null);

        Assert.Equal(5, svc.CategoryId);
    }

    [Fact]
    public void Handle_WithDescription_PropagatesDescription()
    {
        var svc = new RunningTimerService();
        var handler = new StartTimerHandler(svc);

        handler.Handle(null, "Planning session");

        Assert.Equal("Planning session", svc.Description);
    }

    [Fact]
    public void Handle_WithNullCategoryAndDescription_DoesNotThrow()
    {
        var svc = new RunningTimerService();
        var handler = new StartTimerHandler(svc);

        var ex = Record.Exception(() => handler.Handle(null, null));

        Assert.Null(ex);
    }
}
