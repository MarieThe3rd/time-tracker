# AI Usage Reporting Requirements

This document captures the current AI usage reporting requirements after the AI-specific dashboarding was merged into the main Dashboard and Reports experiences.

## Functional Requirements

Dashboard integration: The main dashboard should surface AI impact alongside time and productivity, including weekly AI time saved and assisted-entry counts.

Reports integration: The main Reports page should include an `AI Insights` tab with a shared date-range filter, weekly grouping, charting, and qualitative insights.

Review export integration: The Review Export markdown should include the same AI usage summary model used by the weekly summary so review documents reflect AI-assisted work in the selected period.

Weekly grouping: AI usage data should be grouped by ISO week, with each row showing week start, AI-assisted entry count, time saved, value added, and notes.

Date filtering: Users should be able to filter AI reporting by date range from the shared Reports page controls.

Standalone AI export: Users should still be able to export the AI usage report markdown directly from the integrated Reports experience.

## Non-Functional Requirements

- The integrated reporting experience should remain responsive across desktop and mobile layouts.
- AI reporting logic should stay in report handlers and export services rather than being embedded in Razor UI code.
- Changes should be covered by automated tests, including unit coverage for export formatting and UI coverage for integrated report navigation where feasible.
- Documentation should reflect that AI usage is no longer a standalone page-level feature.

## Current Behavior

| Area                        | Behavior                                                                                         |
| --------------------------- | ------------------------------------------------------------------------------------------------ |
| Dashboard                   | Shows weekly AI time saved, today’s AI time saved, and assisted-entry count in the summary area  |
| Reports                     | AI Insights is exposed as a tab within the main Reports page                                     |
| Weekly summary markdown     | Includes AI totals and a weekly AI usage summary table when AI-assisted entries exist            |
| Review export markdown      | Includes the same AI usage summary section when AI-assisted entries exist in the selected period |
| AI-specific markdown export | Still available as a separate downloadable/exportable markdown artifact from the Reports page    |

## Known Constraints / Assumptions

- Weekly grouping still uses the same week-start logic as the reporting handler.
- Review Export only includes AI summary data when AI-assisted entries exist in the selected range.
- AI-specific vault export remains a separate markdown file even though the UI is now integrated.
