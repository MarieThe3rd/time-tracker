# Task Manager Test Strategy

## Test layers

### Unit tests
Cover:

- task creation rules
- task update rules
- status transitions
- date validation
- report filtering logic
- markdown rendering logic

### Component tests
Cover:

- create task form validation
- edit task form behavior
- list filtering interactions

### Integration tests
Cover:

- persistence mapping
- export generation end-to-end

## Priority scenarios

1. Create task with only required fields
2. Create task with all optional fields
3. Move task to Done and verify completed date behavior
4. Prevent invalid date combinations
5. Filter active tasks correctly
6. Exclude archived tasks from default active view
7. Export due-soon report to markdown deterministically
8. Export ideas and squirrels without losing notes

## Regression focus

- null date handling
- timezone or culture-sensitive date formatting
- archived items leaking into active lists
- completed date not set or overwritten incorrectly
