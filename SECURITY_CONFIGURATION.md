# Secure configuration

TaskFlow does not store deployment credentials in tracked configuration files. Supply secrets at runtime through environment variables or your deployment secret store:

- `Jwt__SecretKey` — a newly generated random value of at least 32 characters.
- `ConnectionStrings__DefaultConnection` — the deployment database connection string.
- `Smtp__Username` and `Smtp__Password` — newly issued SMTP credentials when email is enabled.

The API intentionally refuses to start when `Jwt__SecretKey` is absent or too short. Do not reuse any credential that has previously appeared in Git history. Revoke it at its provider first, then purge the old value from Git history in a coordinated maintenance operation before pushing rewritten history.

For local development, prefer .NET user secrets:

```powershell
dotnet user-secrets set "Jwt:SecretKey" "<new-random-development-key>" --project TaskFlow.API
```
