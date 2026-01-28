

Use exceptions only for exceptional cases, never for validation or normal flow.

Keep try/catch blocks minimal and focused.

Catch specific exceptions; catch Exception only at application boundaries.

Never swallow exceptions.

Rethrow using throw; (never throw ex;).

Log once per failure, preferably at the boundary layer.

Use ILogger<T> with structured logging (no string concatenation).

Use correct log levels (Warning ≠ Error).

Do not log sensitive data.

Lower layers throw; controllers, middleware, and workers handle and log.

Prefer global exception handling and ProblemDetails.

Always respect async/await and CancellationToken.