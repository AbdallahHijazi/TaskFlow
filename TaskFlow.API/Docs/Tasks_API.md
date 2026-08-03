# Tasks API — Models and Returned Details

This document describes the data models used when creating and updating a Task and the shape of Task objects returned by the API (GetAll and GetById).

Keep these DTOs consistent between client and server. Use the types and validation rules below to implement request/response models and mapping to your domain entities.

---

## TaskCreateDto (used for POST /api/tasks)

Purpose: payload submitted by clients to create a new task.

Fields:
- title (string, required) — short title of the task. Max length: 200.
- description (string, optional) — longer description or notes.
- status (string or enum, optional) — initial status (e.g., "Todo", "InProgress", "Done"). Default: "Todo".
- priority (string or enum, optional) — e.g., "Low", "Medium", "High". Default: "Medium".
- dueDate (string, optional) — ISO 8601 date/time for the due date (e.g., "2026-06-13T23:59:59Z"). Nullable.
- assigneeId (GUID/string, optional) — id of a user the task is assigned to. Nullable.
- tags (array of strings, optional) — freeform tags or labels.
- attachments (array of objects, optional) — references to uploaded files. Minimal shape: [{ "id": "guid-or-provider-id", "url": "https://...", "fileName": "..." }]

Example JSON:
{
  "title": "Implement authentication",
  "description": "Add JWT auth and login endpoint",
  "status": "Todo",
  "priority": "High",
  "dueDate": "2026-07-01T00:00:00Z",
  "assigneeId": "c56a4180-65aa-42ec-a945-5fd21dec0538",
  "tags": ["backend", "auth"],
  "attachments": [{ "id": "img_abc123", "url": "https://.../img.png", "fileName": "spec.png" }]
}

Validation recommendations:
- title: required, trim, length 1..200
- status/priority: validate against allowed enum values
- dueDate: must be a valid ISO date or null
- assigneeId: validate GUID format when present

---

## TaskUpdateDto (used for PUT /api/tasks/{id} or PATCH depending on API style)

Purpose: payload to update an existing Task. Fields follow the same types as TaskCreateDto.

Behavior recommendations:
- Allow partial updates when using PATCH (only included fields are updated).
- For PUT, require the full resource representation; missing fields either reset or validated by your chosen semantics.
- Validate values the same as TaskCreateDto.

Example JSON (PUT/PATCH):
{
  "title": "Implement authentication (updated)",
  "description": "Add JWT auth, login and refresh endpoints",
  "status": "InProgress",
  "priority": "High",
  "dueDate": "2026-07-03T12:00:00Z",
  "assigneeId": "c56a4180-65aa-42ec-a945-5fd21dec0538",
  "tags": ["backend", "auth"],
  "attachments": [{ "id": "img_abc123", "url": "https://.../img.png", "fileName": "spec.png" }]
}

---

## TaskDto (returned by GET /api/tasks and GET /api/tasks/{id})

Purpose: canonical read model sent to clients. Contains computed or relational information useful to the client.

Fields:
- id (GUID/string) — unique identifier of the task.
- title (string)
- description (string)
- status (string/enum)
- priority (string/enum)
- createdAt (string, ISO 8601) — creation timestamp.
- updatedAt (string, ISO 8601) — last modified timestamp.
- dueDate (string, ISO 8601, nullable)
- assigneeId (GUID/string, nullable)
- assignee (object, optional) — light user info if you want to include (id, displayName, email). Only include when you don't want clients to call user endpoints separately.
- tags (array of strings)
- attachments (array of objects) — e.g. [{ "id": "img_abc123", "url": "https://...", "fileName": "spec.png", "size": 102400 }]
- isArchived / isDeleted (boolean, optional) — if soft delete or archive is supported
- customFields (object, optional) — for extensibility if you support dynamic fields

Example GET (single item) JSON:
{
  "id": "8f14e45f-ea3b-4c3f-9f4c-4b2a8f6d9f5a",
  "title": "Implement authentication",
  "description": "Add JWT auth and login endpoint",
  "status": "InProgress",
  "priority": "High",
  "createdAt": "2026-06-01T10:00:00Z",
  "updatedAt": "2026-06-10T15:30:00Z",
  "dueDate": "2026-07-01T00:00:00Z",
  "assigneeId": "c56a4180-65aa-42ec-a945-5fd21dec0538",
  "assignee": { "id": "c56a4180-65aa-42ec-a945-5fd21dec0538", "displayName": "Sami Hammadi", "email": "sami@example.com" },
  "tags": ["backend","auth"],
  "attachments": [{ "id": "img_abc123", "url": "https://.../img.png", "fileName": "spec.png", "size": 102400 }],
  "isArchived": false
}

---

## GET /api/tasks (GetAll) — list shape and pagination

Return a list of TaskDto items. Prefer a paginated response to avoid large result sets.

Common pagination envelope:
{
  "items": [ /* TaskDto[] */ ],
  "page": 1,
  "pageSize": 20,
  "totalItems": 123,
  "totalPages": 7
}

Query parameters commonly supported:
- page (int, optional)
- pageSize (int, optional)
- search (string, optional) — search in title/description
- status (string, optional)
- assigneeId (GUID, optional)
- sortBy (string, optional) — e.g., "createdAt", "dueDate"
- sortDir (string, optional) — "asc" or "desc"
- tags (string, optional) — comma-separated tags to filter

Example response (paginated):
{
  "items": [ /* array of TaskDto */ ],
  "page": 1,
  "pageSize": 20,
  "totalItems": 45,
  "totalPages": 3
}

If you do not need pagination, return an array of TaskDto directly.

---

## Additional notes and mapping guidance

- Domain entity: Task should store core fields and relations (assigneeId, attachments, tags) and provider-specific attachment ids if storing externally (e.g., ExternalFileId).
- Persist timestamps (CreatedAt, UpdatedAt) in UTC.
- For attachments, store both URL (public) and provider id to support deletion if required.
- Use proper DTOs for input validation and avoid binding domain entities directly to API models.
- Return consistent date-time format (ISO 8601, UTC) and document it in API docs.

---

If you want, I can also generate C# DTO classes for these models and example AutoMapper profiles compatible with your TaskFlow.Application project. Let me know if you prefer full class definitions and validation attributes.