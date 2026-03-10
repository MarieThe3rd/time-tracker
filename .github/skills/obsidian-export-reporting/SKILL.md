---
name: obsidian-export-reporting
description: Guidance for exporting task and time tracking information into an Obsidian vault as Markdown-friendly reports. Use this when building export, reporting, or markdown formatting features.
---

Use this skill when generating exports or reports intended for an Obsidian vault.

## Output principles

- Produce plain markdown files.
- Use stable headings and bullet structure.
- Prefer filenames that sort well lexicographically.
- Avoid fragile wiki-link assumptions unless the user explicitly wants them.
- Keep content easy to diff in git.

## Suggested report types

- task backlog snapshot
- tasks due soon
- completed tasks by date range
- ideas and squirrels inbox
- deliverables by owner or target

## Suggested export format

Each task entry should support:

- title
- type
- status
- created
- started
- due
- remind
- completed
- deliverable to
- tags
- notes

## Example task markdown

```md
## Prepare March status report
- Type: ToDo
- Status: Active
- Created: 2026-03-09
- Start: 2026-03-10
- Due: 2026-03-14
- Remind: 2026-03-13
- Deliverable To: Leadership
- Tags: #work #reporting
- Notes: Pull time tracking totals and summarize blockers.
```

## File naming guidance

Prefer patterns like:

- `task-report-YYYY-MM-DD.md`
- `tasks-due-YYYY-MM-DD.md`
- `completed-tasks-YYYY-MM-DD-to-YYYY-MM-DD.md`

## Implementation guidance

- Isolate markdown rendering behind an interface.
- Test formatting deterministically.
- Use culture-stable date formatting for export.
- Keep report filtering logic separate from file writing logic.
