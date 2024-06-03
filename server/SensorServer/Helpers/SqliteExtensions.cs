using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using SQLitePCL;

namespace SensorServer.Helpers;

public static class SqliteExtensions
{
    public static async Task EnableWal(this DatabaseFacade database,
        long? maxJournalSize = null,
        CancellationToken cancellationToken = default)
    {
        if (database.GetDbConnection() is not SqliteConnection sqliteConn) return;
        await sqliteConn.OpenAsync(cancellationToken);
        await using var cmd = sqliteConn.CreateCommand();
        cmd.CommandText = "PRAGMA journal_mode=WAL;";
        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        if (result is not string stringResult || !stringResult.Equals("wal", StringComparison.CurrentCultureIgnoreCase))
            throw new InvalidOperationException("Failed to enable WAL mode");
        if (maxJournalSize.HasValue)
        {
            cmd.CommandText = $"PRAGMA journal_size_limit={maxJournalSize.Value};";
            result = await cmd.ExecuteScalarAsync(cancellationToken);
            if (result is not long longResult || longResult != maxJournalSize.Value)
                throw new InvalidOperationException("Failed to set journal size limit");
        }
    }

    public static async Task<(int, int)?> CheckpointDb(this DatabaseFacade database,
        bool passive = false,
        CancellationToken cancellationToken = default)
    {
        if (database.GetDbConnection() is not SqliteConnection sqliteConn) return null;
        await sqliteConn.OpenAsync(cancellationToken);
        var result = raw.sqlite3_wal_checkpoint_v2(sqliteConn.Handle,
            null,
            passive ? raw.SQLITE_CHECKPOINT_PASSIVE : raw.SQLITE_CHECKPOINT_TRUNCATE,
            out var logSize, out var framesCheckPointed);
        if (result != raw.SQLITE_OK)
            throw new InvalidOperationException($"Failed to checkpoint database: {result}");
        return (logSize, framesCheckPointed);
    }
}
