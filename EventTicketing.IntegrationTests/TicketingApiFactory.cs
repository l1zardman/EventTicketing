using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;

namespace EventTicketing.IntegrationTests;

// Boots the real application in memory, pointed at a throwaway SQLite file so each
// test run gets a clean, real database — real EF, real pipeline, real interceptor.
public sealed class TicketingApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"tickets-test-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // The app reads ConnectionStrings:Default during builder setup, BEFORE the host
        // is built — so ConfigureAppConfiguration (which only applies at Build time) is
        // too late to override it and the app falls back to appsettings' shared file.
        // UseSetting injects the value early enough, so every run gets its own fresh DB.
        builder.UseSetting("ConnectionStrings:Default", $"Data Source={_dbPath}");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        // SQLite keeps pooled connections open even after the DbContexts are disposed,
        // which holds a lock on the file. Release the pool first, then clean up the
        // throwaway database (and its WAL/SHM sidecars). If the OS is still holding it,
        // don't fail the test run — it's a temp file and will be reclaimed anyway.
        SqliteConnection.ClearAllPools();
        foreach (var path in new[] { _dbPath, _dbPath + "-wal", _dbPath + "-shm" })
        {
            try { if (File.Exists(path)) File.Delete(path); }
            catch (IOException) { }
        }
    }
}
