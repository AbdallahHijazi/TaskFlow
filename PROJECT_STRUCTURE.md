# TaskFlow - Project Structure

This file documents the repository layout, important projects/files, configuration and deployment notes for TaskFlow (ASP.NET Core 9).

> Keep secrets out of source control. Use environment variables in production.

## Top-level layout

- TaskFlow.sln (solution)
- TaskFlow.API/ (ASP.NET Core Web API - presentation)
- TaskFlow.Application/ (application layer - MediatR, DTOs, features)
- TaskFlow.Domain/ (domain entities, exceptions, common types)
- TaskFlow.Infrastructure/ (EF Core, persistence, storage, DI)
- TaskFlow.Tests/ (unit/integration tests)

## Projects

### TaskFlow.API
- Purpose: HTTP API, controllers, middleware and program startup.
- Key files:
  - Program.cs - application startup, CORS, authentication, PORT handling for Render.
  - DependencyInjection.cs - API-specific DI like JWT and Swagger.
  - Controllers/* - feature controllers (ImagesController, TasksController, UsersController, etc.).
  - appsettings.json / appsettings.Development.json - non-sensitive defaults (sensitive values must be provided as env vars).
  - Infrastructure/ExceptionHandlingMiddleware.cs - centralized error handling.
  - Controllers/Lookup/LookupController.cs - static list of endpoints (lookup)

### TaskFlow.Application
- Purpose: application layer, DTOs, commands/queries, handlers and behaviors.
- Key folders:
  - DTOs/ - data transfer objects for entities
  - Features/ - MediatR commands/queries and handlers
  - Common/ - interfaces (IImageFileStorage, IRepository), models (ImageStorageSaveResult, ImageFileStreamResult)

### TaskFlow.Domain
- Purpose: domain entities and domain-specific exceptions.
- Key files:
  - Entities/ (User, Image, TaskItem, Initiative, Status, Role, ...)
  - Exceptions/ (BadRequestException, NotFoundException)

### TaskFlow.Infrastructure
- Purpose: implementation details: EF Core DbContext, repositories, storage providers, DI wiring.
- Key files:
  - Persistence/AppDbContext.cs
  - Persistence/Repositories/GenericRepository.cs
  - Storage/LocalImageFileStorage.cs - local storage for development
  - Storage/ImageKitImageFileStorage.cs - ImageKit-based storage (production)
  - DependencyInjection.cs - registers DbContext, repositories, IImageFileStorage selection

### TaskFlow.Tests
- Purpose: unit and integration tests.

## Image/file storage
- Production: ImageKit via `TaskFlow.Infrastructure/Storage/ImageKitImageFileStorage.cs`.
- Development: `LocalImageFileStorage` writes to `wwwroot/uploads/images` (avoid using in Render production).
- IImageFileStorage interface: TaskFlow.Application/Common/Interfaces/IImageFileStorage.cs
- Image save result: TaskFlow.Application/Common/Models/ImageStorageSaveResult.cs

## Important endpoints
- API root: `/api`
- Images: `/api/images` (POST upload, GET list, GET {id}/file etc.)
- Lookup endpoints: `/api/lookup` (static list of resources and endpoints)
- Diagnostics (development only): `/api/diagnostics/imagekit`

## Environment variables (recommended for Render)
- SQLSERVER_CONNECTION_STRING (connection string for Somee SQL Server)
- PORT (provided by Render)
- ImageKit settings (preferred names used by code):
  - ImageKit__PublicKey (or IMAGEKIT_PUBLICKEY)
  - ImageKit__PrivateKey (or IMAGEKIT_PRIVATEKEY)
  - ImageKit__UrlEndpoint (or IMAGEKIT_URLENDPOINT) — must start with `https://ik.imagekit.io/`
- IMAGE_MAX_BYTES (optional, default 10MB)
- IMAGE_STORAGE_PROVIDER (optional, "ImageKit" to force ImageKit)
- Jwt secrets and other sensitive config should be set via env vars (e.g., Jwt:SecretKey)

## How local development works
1. Copy `appsettings.Development.json` and set dev-friendly values.
2. Use LocalImageFileStorage by leaving IMAGE_STORAGE_PROVIDER unset (or set to Local).
3. Run `dotnet build` and `dotnet run --project TaskFlow.API` or start via Visual Studio.
4. Use Swagger UI (enabled in Program.cs) during development.

## Production / Render notes
- Provide required env vars in Render dashboard (SQLSERVER_CONNECTION_STRING, ImageKit keys, PORT, Jwt secrets).
- The application reads env vars with both configuration-binding style (ImageKit__PublicKey) and legacy uppercase names.
- The DI layer defaults to ImageKit in Production to prevent writing local files in ephemeral environments.
- Consider running EF migrations via CI/CD or enable optional automatic migration on startup (not currently enabled by default).

## Logging and diagnostics
- ImageKitImageFileStorage logs detailed upload errors and diagnostic info (file name, content type, size, url endpoint) but never prints private keys.
- Diagnostics endpoint available in Development only: GET `/api/diagnostics/imagekit` shows presence of ImageKit keys (without exposing private key).

## Recommended next steps
- Add CI/CD step to run EF migrations to the production DB.
- Add integration tests for ImageKit upload using a test ImageKit account or mocked HttpClient.
- Add retention/deletion logic for external storage (store `ExternalFileId` and implement Delete via ImageKit API).

If you want, I can also:
- Generate a more detailed Class/Component diagram or an OpenAPI summary of all controllers.
- Export a README.md with deployment steps for Render (including exact environment variable names and sample Render service configuration).

