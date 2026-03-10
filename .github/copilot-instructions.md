# Copilot Instructions — time-tracker

This is a **Blazor 10 / .NET** time tracking application. Squad CLI is used as a dev-tool for multi-agent workflows via `package.json` — it is not the app itself.

---

## Squad CLI (time-tracker)

The `time-tracker` project uses `@bradygaster/squad-cli` as a dev dependency. Squad is a programmable multi-agent runtime for GitHub Copilot — it lets you define an AI team, assign roles, and delegate work through an interactive shell.

### Requirements

- Node.js ≥ 20
- GitHub CLI (`gh`) — required for issue/PR operations
- GitHub Copilot

### Running Squad

```bash
# Initialize Squad in the repo (first time only — creates .squad/)
npx squad init

# Launch the interactive shell (talk to your team)
npx squad

# Create an agent
npx squad hire --name <name> --role "<role>"

# Scan for work
npx squad triage

# Continuous work loop
npx squad loop
```

### Squad Team Context

Once initialized, Squad stores its state under `.squad/` at the repo root:

| File                              | Purpose                                                       |
| --------------------------------- | ------------------------------------------------------------- |
| `.squad/team.md`                  | Team roster, member roles, and Copilot capability profile     |
| `.squad/routing.md`               | Work routing rules — which agent handles which type of issue  |
| `.squad/decisions.md`             | Shared team decisions                                         |
| `.squad/agents/{name}/charter.md` | Per-agent domain expertise, coding style, and ownership areas |

**Before starting any issue**, read `.squad/team.md` and `.squad/routing.md`. If the issue has a `squad:{member}` label, read that member's charter and work in their voice.

### Capability Self-Check

Check the **Coding Agent → Capabilities** section in `.squad/team.md` before picking up work:

- 🟢 **Good fit** — proceed autonomously.
- 🟡 **Needs review** — proceed, but flag in the PR description for squad review.
- 🔴 **Not suitable** — do NOT start. Comment on the issue explaining why and suggest reassignment.

### Branch Naming

```
squad/{issue-number}-{kebab-case-slug}
```

Example: `squad/12-add-time-entry-form`

### PR Guidelines

- Reference the issue: `Closes #{issue-number}`
- If the issue had a `squad:{member}` label: `Working as {member} ({role})`
- For 🟡 tasks, add: `⚠️ This task was flagged as "needs review" — please have a squad member review before merging.`
- Check `.squad/decisions.md` for any conventions that apply.

### Recording Decisions

If you make a decision other agents should know about, write it to:

```
.squad/decisions/inbox/copilot-{brief-slug}.md
```

The Scribe agent will merge it into the shared decisions file.

---

## Coding Standards (time-tracker)

### C\# Conventions

- Follow [Microsoft C# coding conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- `PascalCase` — types, methods, properties, constants; `camelCase` — locals/parameters; `_camelCase` — private fields
- `async`/`await` throughout — never `.Result` or `.Wait()`
- Nullable reference types enabled; file-scoped namespaces

### Project Structure — Vertical Slice Architecture

Each slice is self-contained: Blazor component(s) + handler class + slice-local models. No shared service layer. No MediatR.

```
TimeTracker.Web/
  Features/
    Timer/
      TimerPage.razor              ← page
      RunningTimerService.cs       ← background timer state
      StartTimerHandler.cs         ← DbContext injected directly
      StopTimerHandler.cs
      GetTodayEntriesHandler.cs
      DeleteTimeEntryHandler.cs
      UpdateTimeEntryHandler.cs
      UpdateProductivityHandler.cs
      ManualEntry/
        ManualEntryHandler.cs
    Journal/
      JournalChangeService.cs
      AddEntry/
        QuickAddPanel.razor
        AddEntryHandler.cs
      ListEntries/
        JournalPage.razor
        ListEntriesHandler.cs
        DeleteJournalEntryHandler.cs
    Dashboard/
      DashboardPage.razor
      DashboardHandler.cs
    Reports/
      MarkdownExportService.cs     ← builds Markdown, writes to vault
      DailyNote/
        ReportsPage.razor
        ReportsHandler.cs
      ReviewExport/                ← placeholder (not yet implemented)
    Settings/
      SettingsPage.razor
      SettingsHandler.cs
  Data/
    AppDbContext.cs
    Models/                      ← EF entities only (shared across slices)
  Shared/
    Layout/                      ← MainLayout, NavMenu
    Components/                  ← truly shared UI (StarRatingPicker, MarkdownPreview)
  wwwroot/
```

### Testing

- **Unit tests:** xUnit in `TimeTracker.Tests/` — built-in `Assert` only, no FluentAssertions (commercial license)
  - Handler classes tested with EF Core in-memory provider (`UseInMemoryDatabase`)
  - Mirror `Features/` slice structure in test project
  - Run one test: `dotnet test --filter "FullyQualifiedName~ClassName.MethodName"`
  - Run all unit tests: `dotnet test src/TimeTracker.Tests/`
- **UI tests:** Playwright with xUnit in `TimeTracker.UITests/`
  - Use `Microsoft.Playwright` NuGet package (not the NUnit variant)
  - Page Object Model pattern — one class per page/major component
  - Run UI tests: `dotnet test src/TimeTracker.UITests/`
  - Install browsers once after clone: `pwsh bin/Debug/net10.0/playwright.ps1 install`

### Blazor Conventions

- Use `@code { }` blocks unless logic is substantial (then code-behind is acceptable)
- Parameters: `[Parameter]` with PascalCase names; event callbacks named `On{Event}`
- Inject services with `@inject`; call `StateHasChanged()` only when necessary (e.g. timer ticks)

---

# Repository AI instructions

You are working in a Blazor 10 time tracking application written in C# on .NET.

## Core delivery rules

- Prefer small, safe, reviewable changes.
- Preserve working behavior unless the requirement explicitly changes it.
- Do not invent business rules. If a rule is missing, document the assumption in the relevant requirements or architecture document.
- Before making cross-cutting changes, inspect existing patterns in the codebase and follow them unless they are clearly harmful.
- Favor clarity over cleverness.

## Architecture rules

- Keep domain logic out of UI components when possible.
- Prefer explicit domain models, service abstractions, and DTOs over weakly structured objects.
- Keep persistence concerns separate from domain rules.
- Add feature code in vertical slices where practical.
- Design for future export and reporting needs.

## Blazor and .NET rules

- Use async APIs where appropriate.
- Use dependency injection consistently.
- Prefer strongly typed models and enums.
- Use validation on input models.
- Handle nullability intentionally.
- Keep component code-behind and services readable and testable.

## Testing rules

- New behavior requires tests.
- Bug fixes require a regression test when feasible.
- Add unit tests for domain logic.
- Add component or integration tests for critical flows when feasible.
- Do not mark work complete if tests are missing or broken.

## Documentation rules

- Update requirements, architecture notes, and user-facing behavior docs when behavior changes.
- Add concise XML comments only where they improve maintainability.
- Keep markdown documentation factual and current.

## Review rules

- Perform a peer-review style pass before declaring work complete.
- Look for missing tests, business rule mismatches, null issues, naming problems, hidden coupling, and export/reporting impacts.
- Report risks and follow-up items explicitly.

## Task Manager epic focus

When working on task management features:

- Support multiple task types and statuses.
- Support create, start, due, and reminder dates.
- Support export to an Obsidian vault as markdown-friendly output.
- Keep reporting and filtering in scope from the start.
- Consider accessibility and keyboard-friendly workflows.
