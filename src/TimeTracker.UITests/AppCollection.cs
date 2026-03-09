using TimeTracker.UITests.Infrastructure;

namespace TimeTracker.UITests;

/// <summary>
/// Defines the "App" xUnit collection. All test classes tagged [Collection("App")]
/// share the same AppFixture — one server process and one Playwright browser for the
/// entire test run, which is much faster than starting a new process per class.
/// </summary>
[CollectionDefinition("App")]
public class AppCollection : ICollectionFixture<AppFixture>;
