# AI Usage Reports Feature
This document outlines the requirements for the AI Usage Reports feature in Time Tracker. This feature will provide insights into how users are leveraging AI assistance within the app, including usage patterns, time saved, and potential areas for improvement.

## Functional requirements
The ai usage report should create a new md file to export into the obsidian vault with the following content:
```markdown
# AI Usage Report for {{date range}}
| Week Start | Week End | AI-Assisted Work Done | Value Added | Time Saved (minutes) | Notes |
|------|------|-----------------------|-------------|----------------------|-------|
| {{week start date}} | {{week end date}} | {{count of tasks with AI assistance}} | {{qualitative value assessment}} | {{sum of AiTimeSavedMinutes for tasks in this week}} | {{any notable patterns or insights}} |
```

## Non-functional requirements
- The report should be exportable as a markdown file suitable for inclusion in an Obsidian vault.
- The implementation should be maintainable and follow the coding standards outlined in the Ethan principal developer charter.
- The feature should be covered by automated tests, including unit tests for any new domain logic and component/integration tests for critical flows when feasible.
- Documentation and requirements should be updated to reflect any changes in behavior, and XML comments should be added where they improve maintainability.

## Implementation Notes

- **Branch:** `feature/ai-usage-reports`
- **Status:** Implemented ✅

### Key decisions

| Decision | Detail |
|---|---|
| Weekly aggregation model | `AiUsageWeeklyItem` with fields `WeekStart`, `WeekEnd`, `AiTaskCount`, `TotalTimeSavedMinutes`, `ValueAdded`, `Notes` |
| Week boundary | ISO week, Monday start |
| Grouping strategy | In-memory after EF fetches all entries matching the date range — no database-level `GROUP BY` |
| Pipe escaping | Done at render time inside `BuildAiUsageWeeklyReport`; pipe characters in `ValueAdded` and `Notes` are replaced with `\|` before writing into the markdown table |
| Output filename | `{VaultRootPath}/AI-Usage-Report-{from}-to-{to}.md` |
| Overwrite detection | `PushAiUsageReportAsync` returns `(string Path, bool Overwritten)`; callers can inspect the flag but no confirmation dialog is shown |

### Known constraints / assumptions

- Week grouping operates on the **UTC date** of each entry's stored `StartTime`. Users in timezones that differ significantly from UTC may see entries fall into an unexpected week bucket near midnight boundaries.
- Overwrite is silent beyond the returned `Overwritten` flag — there is no UI confirmation dialog before an existing vault file is replaced.
- Pipe escaping is applied at render time only; the raw `ValueAdded` and `Notes` values stored in memory retain their original characters.