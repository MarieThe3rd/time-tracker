# Task Manager Architecture Notes

## Recommended design shape

Use a layered or vertical-slice design that separates:

- UI components
- application services or handlers
- domain models and rules
- infrastructure or persistence
- export/report rendering

## Core domain suggestions

### TaskItem
Represents one tracked item.

Suggested properties:

- Id
- Title
- Notes
- Type
- Status
- CreatedOn
- StartOn
- DueOn
- RemindOn
- CompletedOn
- DeliverableTo
- Tags

### TaskType
Prefer enum initially.

Suggested values:

- ToDo
- WantToDo
- Reminder
- Idea
- Squirrel

### TaskStatus
Prefer enum initially.

Suggested values:

- New
- Active
- Waiting
- Deferred
- Done
- Cancelled
- Archived

## Services

### ITaskService
Owns create, update, transition, and retrieval operations.

### ITaskReportService
Owns report filtering and report projections.

### IMarkdownExportService
Owns markdown rendering and export packaging for Obsidian.

## Validation guidance

- CreatedOn required and system-generated
- CompletedOn required when status is Done
- CompletedOn cleared or rejected for non-terminal states based on business rules
- RemindOn can exist without DueOn
- StartOn should not be after CompletedOn
- DueOn before CreatedOn should be rejected unless existing legacy data requires a softer rule

## Reporting guidance

Build report projections instead of passing raw entities directly into export templates.

## Migration guidance

If this app already persists time entries, keep task persistence independent enough that time tracking can later link to tasks without tightly coupling the two concepts.
