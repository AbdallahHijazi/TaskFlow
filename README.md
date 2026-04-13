# TaskFlow

## User Tasks API Contract (MVP)

Endpoint:

- `GET /api/users/{id}/tasks`

### Authorization Rules

- Requires authenticated user (`Bearer JWT`).
- Access is allowed when:
  - The requester is the same user as route `{id}`, or
  - The requester has role `Admin` or `Manager`.
- Otherwise the API returns `403 Forbidden`.

### Query Parameters

- `pageNumber` (int, default `1`, min `1`)
- `pageSize` (int, default `20`, range `1..100`)
- `status` (Guid?, optional)
- `initiativeId` (Guid?, optional)
- `priority` (int?, optional, range `1..5`)
- `isOverdue` (bool?, optional)
- `fromDate` (DateTime?, optional)
- `toDate` (DateTime?, optional, must be `>= fromDate` when both provided)
- `search` (string?, optional; searches task name, description, and initiative name)
- `sortBy` (string, default `createdAt`, allowed: `createdAt`, `dueDate`, `priority`)
- `sortDirection` (string, default `desc`, allowed: `asc`, `desc`)

### Successful Response (200)

Returns paged result:

- `pageNumber`
- `pageSize`
- `totalCount`
- `totalPages`
- `items`

### Unified Error Response

All handled API errors follow this structure:

- `message`: human-readable message
- `errorCode`: machine-readable code
- `validationErrors`: field-level errors dictionary (present for validation errors)
- `traceId`: request trace identifier

Example:

```json
{
  "message": "Validation failed.",
  "errorCode": "VALIDATION_ERROR",
  "validationErrors": {
    "Parameters.SortBy": [
      "sortBy must be one of: createdAt, dueDate, priority."
    ]
  },
  "traceId": "00-abc123..."
}
```

Common error codes:

- `VALIDATION_ERROR` (400)
- `BAD_REQUEST` (400)
- `UNAUTHORIZED` (401)
- `NOT_FOUND` (404)
- `CONFLICT` (409)
- `INTERNAL_SERVER_ERROR` (500)