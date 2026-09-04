using Microsoft.EntityFrameworkCore;
using T3mmyvsa.Migrations;

var maxAttempts = ReadPositiveInt("MIGRATION_MAX_ATTEMPTS", 30, 100);
var retryDelaySeconds = ReadPositiveInt("MIGRATION_RETRY_DELAY_SECONDS", 2, 60);
var factory = new DesignTimeAppDbContextFactory();

await using (var discoveryContext = factory.CreateDbContext(args))
{
    if (!discoveryContext.Database.GetMigrations().Any())
    {
        Console.Error.WriteLine(
            "No EF Core migrations were found in the migrations project. Generate an initial migration before deployment.");
        return 2;
    }
}

for (var attempt = 1; attempt <= maxAttempts; attempt++)
{
    try
    {
        await using var db = factory.CreateDbContext(args);
        await db.Database.MigrateAsync();
        Console.WriteLine("Database migrations applied successfully.");
        return 0;
    }
    catch (Exception ex) when (attempt < maxAttempts)
    {
        Console.Error.WriteLine(
            $"Migration attempt {attempt}/{maxAttempts} failed: {ex.GetType().Name}. Retrying in {retryDelaySeconds}s.");
        await Task.Delay(TimeSpan.FromSeconds(retryDelaySeconds));
    }
}

Console.Error.WriteLine($"Database migration failed after {maxAttempts} attempts.");
return 1;

static int ReadPositiveInt(string name, int fallback, int max)
{
    var raw = Environment.GetEnvironmentVariable(name);
    return int.TryParse(raw, out var value) && value > 0 && value <= max
        ? value
        : fallback;
}
