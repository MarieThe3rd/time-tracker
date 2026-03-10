---
name: task-manager-domain-design
description: Design guidance for the Task Manager epic in the Time Tracker app. Use this when creating or modifying task domain models, statuses, types, workflows, filtering, and reporting behavior.
---

Use this skill when the work involves task modeling, task workflows, dates, reporting, or future-proofing the task manager.

## Goal

Create a task model that is simple now but extensible later.

## Minimum domain concepts

- Task item
- Task type
- Task status
- Deliverable target
- Reminder metadata
- Export/report model

## Baseline task fields

Include at minimum:

- unique id
- title
- optional description or notes
- task type
- task status
- create date
- optional start date
- optional due date
- optional remind date
- optional completed date
- deliverable to
- optional tags
- optional parent or grouping reference if the codebase supports grouping

## Suggested task types

Start with these:

- ToDo
- WantToDo
- Reminder
- Idea
- Squirrel

Keep the enum extensible.

## Suggested statuses

Start with these:

- New
- Active
- Waiting
- Deferred
- Done
- Cancelled
- Archived

Document allowed transitions.

## Important rules

- A task can exist without a due date.
- Reminder date must not imply due date.
- Completed tasks should capture completed date.
- Archived tasks should not appear in default active lists.
- Filtering and reporting must treat type and status separately.
- Squirrel items are valid inputs, not junk data. They represent diversion ideas and should be storable without forcing full planning.

## Design guidance

- Prefer enums for stable categories.
- If statuses or types need user customization later, isolate mapping and display concerns.
- Keep export projection separate from persistence entities.
- Keep date validation rules centralized.

## Deliverables

When using this skill, produce:

1. domain model recommendation
2. status transition rules
3. validation notes
4. reporting and export implications
