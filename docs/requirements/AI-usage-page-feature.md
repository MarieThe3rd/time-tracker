# AI Usage Page Feature Requirements
This document outlines the requirements for the AI Usage Page feature in Time Tracker. This page will provide users with insights into their AI-assisted work, including usage patterns, time saved, and potential areas for improvement.
## Functional requirements
Chart display: The page should display a chart showing AI usage over time, including the number of tasks with AI assistance and the total time saved (in minutes) for each week.
Data grouping: The data should be grouped by week, with each entry showing the week start date, week end date, count of tasks with AI assistance, and total time saved.
Qualitative insights: The page should include a section for qualitative insights, where users can see any notable patterns or observations about their AI usage.

User should be able to filter the report by date range to see specific periods of AI usage.
## Non-functional requirements  
- The page should be responsive and render well on various screen sizes, including mobile devices.
- The implementation should be maintainable and follow the coding standards outlined in the Ethan principal developer charter
- The feature should be covered by automated tests, including unit tests for any new domain logic and component/integration tests for critical flows when feasible.
- Documentation and requirements should be updated to reflect any changes in behavior, and XML comments should be added where they improve maintainability.

## Implementation Notes

- **Branch:** `feature/ai-usage-reports`
- **Status:** Implemented ✅

### Key decisions

| Decision | Detail |
|---|---|
| Weekly grouping | Page rewritten to display data by ISO week (Monday start), replacing a prior per-entry detail view |
| Weekly table columns | 6 columns: Week Start, Week End, AI-Assisted Tasks, Time Saved (min), Value Added, Notes |
| Chart | Visualises weekly AI task count and total time saved via Chart.js |
| Qualitative insights | Rendered as a dedicated section below the weekly table |
| Export to Obsidian | Button calls `PushAiUsageReportAsync`; result reports the written file path and whether an existing file was overwritten |
| Download | Separate button saves the generated markdown locally without touching the vault |
| Date-range guard | Inverted guard: an invalid or reversed date range disables the Export and Download action buttons rather than blocking the whole page |

### Known constraints / assumptions

- Action buttons (Export, Download) are disabled when the date range is invalid; no page-level error state is displayed.
- Weekly grouping reflects the same UTC week boundaries used by the handler (`GetWeeklyAiUsageAsync`). Users in non-UTC timezones may see entries near midnight shift to an adjacent week.
- There is no UI confirmation dialog before an existing Obsidian vault file is overwritten; the overwrite is indicated only by the `Overwritten` flag returned from the handler.