# TaskFlow — Database Schema and Entity Relationships

This document describes the main database entities, key fields, and relationships used by TaskFlow. It is intended as a reference for developers, migrations, and integration work.

Note: model names and fields are conservative and reflect common patterns present in the codebase (Tasks, Initiatives, Images/Attachments, Users). Adjust names and types to match your actual DbContext/entity classes when generating migrations.

## Conventions
- Primary keys are GUIDs (uniqueidentifier) unless noted otherwise.
- Timestamps are stored in UTC (CreatedAt, UpdatedAt).
- Soft-delete is supported with IsDeleted and/or IsArchived booleans.
- Attachments uploaded to external providers store both public URL and ExternalFileId for provider operations.

## Entities

### Users
- id (GUID) PK
- displayName (string)
- email (string, unique)
- avatarImageId (GUID, nullable) FK -> Images.id
- createdAt, updatedAt
- isDeleted (bool)

Purpose: system users; referenced by tasks, initiatives, projects, comments.

### Initiatives
- id (GUID) PK
- name (string)
- summary (string)
- description (string)
- ownerId (GUID, nullable) FK -> Users.id
- status (string/enum)
- priority (string/enum)
- startDate (datetime, nullable)
- targetEndDate (datetime, nullable)
- progressPercent (int)
- metrics (JSON / nvarchar(max), nullable)
- relatedProjectId (GUID, nullable) FK -> Projects.id (if Projects exist)
- createdAt, updatedAt
- isArchived, isDeleted

Purpose: high-level initiatives that group tasks and goals.

### Tasks
- id (GUID) PK
- title (string)
- description (string, nullable)
- status (string/enum)
- priority (string/enum)
- createdAt, updatedAt
- dueDate (datetime, nullable)
- assigneeId (GUID, nullable) FK -> Users.id
- initiativeId (GUID, nullable) FK -> Initiatives.id
- parentTaskId (GUID, nullable) FK -> Tasks.id (subtask relationship)
- isArchived, isDeleted

Purpose: work items. Tasks can belong to an Initiative and optionally have subtasks.

### Images / Attachments
- id (GUID) PK
- entityType (string) — optional: "Task", "Initiative", "User" etc.
- entityId (GUID) — id of parent entity
- fileName (string)
- url (string) — public URL
- contentType (string)
- sizeInBytes (long)
- externalFileId (string, nullable) — provider id (ImageKit, S3 key)
- uploadedById (GUID, nullable) FK -> Users.id
- createdAt, updatedAt
- isDeleted

Purpose: store file metadata for external storage; attach to tasks, initiatives, users, comments.

### Tags
- id (GUID) PK
- name (string)
- createdAt, updatedAt
- isDeleted

Purpose: label tasks and initiatives.

### TaskTags (join table)
- taskId (GUID) FK -> Tasks.id
- tagId (GUID) FK -> Tags.id
- composite PK (taskId, tagId)

Purpose: many-to-many Tasks <-> Tags.

### InitiativeStakeholders
- id (GUID) PK
- initiativeId (GUID) FK -> Initiatives.id
- userId (GUID) FK -> Users.id
- role (string)

Purpose: list stakeholders and roles for an initiative.

### Comments (optional)
- id (GUID) PK
- entityType (string) — e.g., "Task", "Initiative"
- entityId (GUID) — FK to parent entity
- authorId (GUID) FK -> Users.id
- content (text)
- createdAt, updatedAt
- isDeleted

Purpose: threaded discussion attached to entities.

## Relationships (summary)
- Users 1 --- * Tasks (assigneeId)
- Initiatives 1 --- * Tasks (initiativeId)
- Tasks 1 --- * Tasks (parentTaskId) (self-referencing subtasks)
- Tasks * --- * Tags (via TaskTags)
- Users 1 --- * Images (uploadedById)
- Entities (Tasks/Initiatives/Users/Comments) 1 --- * Images (entityId + entityType)
- Initiatives * --- * Users (via InitiativeStakeholders)

## Indexes and Performance
- Index Users.email (unique)
- Index Tasks.assigneeId, Tasks.initiativeId, Tasks.status
- Index Initiatives.ownerId, Initiatives.status
- Index Images.entityType + entityId (for fast lookup)
- Consider JSON indexes (SQL Server 2016+ functions) if using JSON metrics/metadata

## Referential actions
- On delete for Users: prefer SET NULL for assigneeId/ownerId to preserve history, or restrict depending on policy.
- On delete for parent Task: use cascade for subtasks or restrict and handle in application logic.
- For attachments: keep records even if parent is soft-deleted; use isDeleted to hide.

## Notes for external file storage
- Store url (public) for serving files and externalFileId for delete operations against provider (ImageKit).
- Do not store secrets in DB. Store provider settings in environment variables.

## Migration guidance
- Use EF Core migrations to scaffold tables from entities.
- Keep migration scripts idempotent for CI/CD. Run migrations as part of deployment pipeline.
- When adding new enums, use string-backed columns or a lookup table to avoid migration complications.

If you want, I can generate:
- SQL CREATE TABLE scripts for the entities listed above, or
- EF Core entity classes + Fluent API configuration and migration scaffold files for TaskFlow.Application and TaskFlow.Infrastructure.

