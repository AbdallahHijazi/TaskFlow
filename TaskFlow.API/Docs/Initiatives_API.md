# Initiatives API — Models and Returned Details

This document describes the data models used when creating and updating an Initiative and the shape of Initiative objects returned by the API (GetAll and GetById).

Keep these DTOs consistent between client and server. Use the types and validation rules below to implement request/response models and map to your domain entities.

---

## InitiativeCreateDto (POST /api/initiatives)

Purpose: payload submitted by clients to create a new initiative.

Fields:
- name (string, required) — short name/title of the initiative. Max length: 250.
- summary (string, optional) — one-paragraph summary or objective.
- description (string, optional) — detailed description, scope and acceptance criteria.
- ownerId (GUID/string, optional) — primary responsible user. Nullable.
- status (string or enum, optional) — e.g., "Planned", "Active", "Completed", "OnHold". Default: "Planned".
- priority (string or enum, optional) — e.g., "Low", "Medium", "High". Default: "Medium".
- startDate (string, ISO 8601, optional) — planned start date/time. Nullable.
- targetEndDate (string, ISO 8601, optional) — planned end date/time. Nullable.
- progressPercent (number, optional) — 0-100 progress estimate.
- metrics (object, optional) — key/value pairs for initiative-specific KPIs, e.g. { "kpiRevenue": 10000 }
- relatedProjectId (GUID/string, optional) — link to a project if applicable.
- tags (array of strings, optional)
- stakeholders (array of objects, optional) — minimal: [{ "id": "guid", "role": "Sponsor" }]
- attachments (array of objects, optional) — [{ "id": "provider-id-or-guid", "url": "https://...", "fileName": "..." }]

Validation recommendations:
- name: required, trim, length 1..250
- status/priority: validate against allowed enum values
- dates: valid ISO dates and startDate <= targetEndDate when both present
- ownerId/relatedProjectId: validate GUID when present

Example JSON:
{
  "name": "Q3 Customer Onboarding",
  "summary": "Improve onboarding flow to reduce time-to-value",
  "description": "Work across teams to automate welcome emails and tutorials.",
  "ownerId": "c56a4180-65aa-42ec-a945-5fd21dec0538",
  "status": "Active",
  "priority": "High",
  "startDate": "2026-07-01T00:00:00Z",
  "targetEndDate": "2026-09-30T23:59:59Z",
  "progressPercent": 25,
  "tags": ["onboarding","q3"],
  "attachments": [{ "id": "img_abc123", "url": "https://.../spec.pdf", "fileName": "spec.pdf" }]
}

---

## InitiativeUpdateDto (PUT/PATCH /api/initiatives/{id})

Purpose: payload to update an existing Initiative. Fields follow same types as InitiativeCreateDto.

Behavior recommendations:
- Support partial updates with PATCH (only included fields are modified).
- For PUT require full representation or follow chosen semantics about resets.
- Validate values the same as InitiativeCreateDto.

---

## InitiativeDto (returned by GET /api/initiatives and GET /api/initiatives/{id})

Purpose: canonical read model sent to clients. Include computed and relational info useful to consumers.

Fields:
- id (GUID/string) — unique identifier.
- name (string)
- summary (string)
- description (string)
- ownerId (GUID/string, nullable)
- owner (object, optional) — light user info: { id, displayName, email }
- status (string/enum)
- priority (string/enum)
- startDate (ISO 8601, nullable)
- targetEndDate (ISO 8601, nullable)
- progressPercent (number)
- metrics (object) — KPI key/value map
- relatedProjectId (GUID/string, nullable)
- tags (string[])
- stakeholders (array of light objects) — [{ id, role, name? }]
- attachments (array of objects) — [{ id, url, fileName, size }]
- createdAt (ISO 8601) — creation timestamp (UTC)
- updatedAt (ISO 8601) — last modified timestamp (UTC)
- isArchived / isDeleted (boolean, optional)

Example GET (single item):
{
  "id": "8f14e45f-ea3b-4c3f-9f4c-4b2a8f6d9f5a",
  "name": "Q3 Customer Onboarding",
  "summary": "Improve onboarding flow",
  "description": "...",
  "ownerId": "c56a4180-65aa-42ec-a945-5fd21dec0538",
  "owner": { "id": "c56a4180-65aa-42ec-a945-5fd21dec0538", "displayName": "Sami Hammadi" },
  "status": "Active",
  "priority": "High",
  "startDate": "2026-07-01T00:00:00Z",
  "targetEndDate": "2026-09-30T23:59:59Z",
  "progressPercent": 25,
  "metrics": { "activatedUsers": 1200 },
  "tags": ["onboarding","q3"],
  "attachments": [{ "id": "img_abc123", "url": "https://.../spec.pdf", "fileName": "spec.pdf", "size": 204800 }],
  "createdAt": "2026-06-01T10:00:00Z",
  "updatedAt": "2026-06-10T15:30:00Z",
  "isArchived": false
}

---

## GET /api/initiatives (GetAll) — list shape and pagination

Return a paginated list of InitiativeDto items. Use a pagination envelope to avoid large result sets.

Common pagination envelope:
{
  "items": [ /* InitiativeDto[] */ ],
  "page": 1,
  "pageSize": 20,
  "totalItems": 123,
  "totalPages": 7
}

Query parameters commonly supported:
- page, pageSize
- search (name/summary/description)
- status
- ownerId
- relatedProjectId
- sortBy (e.g., "createdAt", "targetEndDate", "progressPercent")
- sortDir ("asc" or "desc")
- tags (comma-separated)

---

## Additional notes and mapping guidance

- Domain entity: Initiative should store core fields, relations (ownerId, relatedProjectId), KPIs/metrics (as JSON or related table), attachments and provider ids for external storage.
- Persist dates and timestamps in UTC and return ISO 8601 strings.
- For attachments store provider file id (ExternalFileId) and public URL so you can delete externally when needed.
- Use DTOs for validation and avoid binding domain entities directly to API models.
- Consider exposing a small summary DTO for list endpoints (id, name, summary, status, progressPercent, ownerId) to reduce payload size.

---

If you want C# DTO classes and an AutoMapper profile for these models (DataAnnotations or FluentValidation), I can generate them for TaskFlow.Application. Let me know which validation style you prefer.