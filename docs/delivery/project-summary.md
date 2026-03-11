# Time Tracker Project Summary

## Project Overview

Time Tracker is a Blazor 10 / .NET application for tracking work time, journaling outcomes, producing markdown exports, and reporting productivity trends (including AI-assisted work impact).

Primary capabilities include:

- Timer and manual time entry workflows
- Daily and weekly reporting
- Review export generation
- Journal capture for wins, challenges, and learnings
- Dashboard and reports with AI impact insights
- Unit and UI test coverage with xUnit and Playwright

## Repository Snapshot (March 10, 2026)

These values are based on repository analysis commands run against the current workspace.

- Total commits: `25`
- Authored source files (`.cs`, `.razor`, `.js`, excluding `bin`/`obj`): `85`
- Authored source lines (`.cs`, `.razor`, `.js`, excluding `bin`/`obj`): `21,200`
- Test files in `TimeTracker.Tests` and `TimeTracker.UITests`: `37`
- xUnit `[Fact]` tests discovered in test projects: `207`

## Estimated Development Effort Without AI Agents

Estimated effort range:

- Lower bound: `220` hours
- Midpoint: `290` hours
- Upper bound: `380` hours

Suggested planning range:

- Most likely: `260` to `320` hours

## Assumptions Behind The Estimate

- A developer (or small team) builds the current feature set from scratch without AI agent acceleration.
- Includes implementation, debugging, tests, and supporting documentation updates.
- Accounts for normal iteration and refactoring, but not major product pivots.
- Team experience is moderate to strong with Blazor, EF Core, and automated testing.

## Calendar-Time Conversion

At `30` dev-hours/week:

- `220` hours: ~`7.3` weeks
- `290` hours: ~`9.7` weeks
- `380` hours: ~`12.7` weeks

At `40` dev-hours/week:

- `220` hours: ~`5.5` weeks
- `290` hours: ~`7.3` weeks
- `380` hours: ~`9.5` weeks

## Effort Breakdown By Feature Area (Without AI)

The table below distributes the midpoint estimate (`290` hours) across major delivery areas.

| Feature Area                           | Estimated Hours | Notes                                                                        |
| -------------------------------------- | --------------: | ---------------------------------------------------------------------------- |
| Timer + Manual Entry                   |            `48` | Start/stop timer flows, manual entry, edit/update handling, validation rules |
| Journal                                |            `28` | Entry creation, listing, deletion, and UX wiring                             |
| Dashboard                              |            `36` | Summary cards, recent activity, AI impact integration                        |
| Reports + AI Insights                  |            `54` | Date filtering, tabs, weekly AI aggregation, chart integration               |
| Markdown Exports (Daily/Weekly/Review) |            `32` | Export formatting, AI summary integration, file output behavior              |
| Data + Persistence                     |            `24` | EF Core models/context, migration alignment, query shaping                   |
| Automated Testing (Unit + UI)          |            `52` | xUnit coverage, Playwright page objects/tests, regression stabilization      |
| Documentation + Delivery Support       |            `16` | Requirements/design/docs updates, review prep, delivery notes                |
| **Total**                              |       **`290`** | Midpoint of no-AI estimate                                                   |

## Breakdown Range Guidance

- Lower-effort scenario (`220` hours): each area is roughly `75%` to `80%` of the midpoint values.
- Upper-effort scenario (`380` hours): each area is roughly `125%` to `135%` of the midpoint values.
- Highest variance areas: `Automated Testing`, `Reports + AI Insights`, and `Markdown Exports` due to iteration and integration effects.

## Notes

This estimate is intentionally provided as a range instead of a single-point forecast. The largest source of variance is typically test stabilization and requirement iteration across reporting and export workflows.
