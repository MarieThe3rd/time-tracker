# Task Manager Epic

## Epic summary

Add a task manager to the Time Tracker Blazor 10 application so the app can track actionable work, reminders, ideas, and short-lived diversion items in one place, while supporting reporting and markdown export into an Obsidian vault.

## Goals

- Manage more than traditional to-do items.
- Track multiple task types and multiple lifecycle states.
- Support planning with important dates.
- Support reporting and export to Obsidian.
- Deliver production-ready code with tests, docs, and peer review.

## In scope

- create, read, update, and organize tasks
- task types
- task statuses
- create/start/due/remind/completed dates
- deliverable target field
- filtering and reporting views
- markdown export to Obsidian vault
- documentation
- automated tests
- peer review workflow

## Out of scope for this epic

- recurring tasks
- shared multi-user collaboration unless already present in the app
- notifications outside the application unless already supported
- mobile-specific UX beyond responsive web behavior

## Proposed task types

- To Do
- Want To Do
- Reminder
- Idea
- Squirrel

### Notes

- `Squirrel` means a quick diversion, tangent, or idea capture item that should not be lost.
- The system should allow adding more types later without major redesign.

## Proposed task statuses

- New
- Active
- Waiting
- Deferred
- Done
- Cancelled
- Archived

### Status behavior

- `New` is default for freshly created items unless a workflow says otherwise.
- `Done` captures completion.
- `Archived` hides older finished or abandoned items from default active views.
- `Cancelled` means intentionally not being completed.
- `Deferred` means intentionally paused for later.
- `Waiting` means blocked by outside dependency.

## Required fields

- id
- title
- task type
- status
- create date

## Optional fields

- description or notes
- start date
- due date
- remind date
- completed date
- deliverable to
- tags

## Functional requirements

### FR-001 Create task
Users can create a new task with required fields and any supported optional fields.

### FR-002 Edit task
Users can update title, notes, task type, status, dates, deliverable target, and tags.

### FR-003 View lists
Users can view task lists filtered by status, type, due window, and deliverable target.

### FR-004 Status transitions
The system enforces reasonable status transitions and records completion date when moved to Done.

### FR-005 Date handling
The system stores create date automatically and supports optional start, due, remind, and completed dates.

### FR-006 Reminder support
The system stores a reminder date independent of due date.

### FR-007 Deliverable target
The system supports a `deliverable to` field for identifying who or what the work is for.

### FR-008 Obsidian export
Users can export task reports as markdown files suitable for an Obsidian vault. Configurable export options include date range, note location and name, status/type filters, and report format.

### FR-009 Reporting
Users can generate task-oriented reports such as active tasks, tasks due soon, completed tasks over a date range, ideas, and squirrels.

### FR-010 Search and filtering foundation
The design supports future search, grouping, and tagging without major redesign.

### FR-011 Documentation
Requirements, architecture notes, and user-facing behavior documentation are updated with the feature.

### FR-012 Quality gates
Feature delivery includes automated tests and a peer-review pass.

## Non-functional requirements

### NFR-001 Maintainability
Domain rules should be easy to find and test.

### NFR-002 Testability
Non-trivial behavior must be covered by automated tests.

### NFR-003 Accessibility
Core task workflows should be keyboard-usable and accessible.

### NFR-004 Deterministic export
Obsidian export must produce predictable markdown and stable date formatting.

### NFR-005 Extensibility
The design should support future task recurrence, categorization, and richer reporting.

## Suggested stories

### Story 1: domain model and persistence foundation
Create task entities, enums, validation rules, persistence mapping, and migrations if needed.

### Story 2: task CRUD UI
Build forms and screens for creating, editing, and listing tasks.

### Story 3: filtering and reporting queries
Add filters and task report generation logic.

### Story 4: Obsidian markdown export
Create export service, markdown renderer, file packaging or save flow, and tests.

### Story 5: documentation and review hardening
Update docs, finish peer review, and close gaps.

## Acceptance criteria

- Users can create and edit tasks with supported fields.
- Users can classify tasks by type and status.
- Active and archived views behave correctly.
- Due and reminder handling work independently.
- Exported markdown is readable in Obsidian.
- Tests cover task rules and export behavior.
- Docs and peer review artifacts are present.
