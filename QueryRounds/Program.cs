using Microsoft.Data.SqlClient;

var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") ?? 
    throw new InvalidOperationException("CONNECTION_STRING environment variable is required");

using var connection = new SqlConnection(connectionString);
await connection.OpenAsync();

var query = @"
    SELECT TOP 50 
        r.RoundNumber, 
        r.StartTime, 
        r.EndTime, 
        r.WinnerTeam, 
        r.EndReason, 
        ISNULL(d.DemoSource, 'NULL') as DemoSource, 
        d.FileName,
        d.Id as DemoId
    FROM Rounds r 
    INNER JOIN Matches m ON r.MatchId = m.Id 
    INNER JOIN DemoFiles d ON m.DemoFileId = d.Id 
    ORDER BY d.Id, r.RoundNumber";

using var command = new SqlCommand(query, connection);
using var reader = await command.ExecuteReaderAsync();

Console.WriteLine("DemoId | DemoSource | RoundNumber | StartTime | EndTime | WinnerTeam | EndReason | FileName");
Console.WriteLine(new string('-', 120));

while (await reader.ReadAsync())
{
    Console.WriteLine($"{reader["DemoId"],-6} | {reader["DemoSource"],-10} | {reader["RoundNumber"],-11} | {reader["StartTime"],-9:F1} | {reader["EndTime"],-7:F1} | {reader["WinnerTeam"],-10} | {reader["EndReason"],-9} | {reader["FileName"]}");
}

Console.WriteLine("\n--- Analyzing round structure for different demo sources ---");

// Check for earliest rounds by demo source
var structureQuery = @"
    SELECT 
        ISNULL(d.DemoSource, 'NULL') as DemoSource,
        COUNT(DISTINCT d.Id) as DemoCount,
        MIN(r.RoundNumber) as FirstRound,
        MAX(r.RoundNumber) as LastRound,
        AVG(CAST(r.RoundNumber as FLOAT)) as AvgRoundNumber
    FROM Rounds r 
    INNER JOIN Matches m ON r.MatchId = m.Id 
    INNER JOIN DemoFiles d ON m.DemoFileId = d.Id 
    GROUP BY d.DemoSource
    ORDER BY d.DemoSource";

reader.Close();
using var structureCommand = new SqlCommand(structureQuery, connection);
using var structureReader = await structureCommand.ExecuteReaderAsync();

Console.WriteLine("\nDemo Source | Demo Count | First Round | Last Round | Avg Round");
Console.WriteLine(new string('-', 65));

while (await structureReader.ReadAsync())
{
    Console.WriteLine($"{structureReader["DemoSource"],-11} | {structureReader["DemoCount"],-10} | {structureReader["FirstRound"],-11} | {structureReader["LastRound"],-10} | {structureReader["AvgRoundNumber"],-8:F1}");
}

Console.WriteLine("\n--- Examining round characteristics to identify warmup/knife rounds ---");

// Check round characteristics to identify warmup
structureReader.Close();
var roundCharacteristicsQuery = @"
    SELECT 
        r.RoundNumber,
        r.EndReason,
        r.WinnerTeam,
        r.EndTime,
        COUNT(DISTINCT k.Id) as KillCount,
        COUNT(DISTINCT wf.Id) as WeaponFireCount
    FROM Rounds r
    INNER JOIN Matches m ON r.MatchId = m.Id 
    INNER JOIN DemoFiles d ON m.DemoFileId = d.Id
    LEFT JOIN Kills k ON k.RoundId = r.Id
    LEFT JOIN WeaponFires wf ON wf.RoundId = r.Id
    WHERE r.RoundNumber <= 5  -- Look at first 5 rounds
    GROUP BY r.RoundNumber, r.EndReason, r.WinnerTeam, r.EndTime
    ORDER BY r.RoundNumber";

using var charCommand = new SqlCommand(roundCharacteristicsQuery, connection);
using var charReader = await charCommand.ExecuteReaderAsync();

Console.WriteLine("Round | EndReason | Winner | HasEndTime | Kills | WeaponFires");
Console.WriteLine(new string('-', 65));

while (await charReader.ReadAsync())
{
    var hasEndTime = charReader.IsDBNull(charReader.GetOrdinal("EndTime")) ? "NO" : "YES";
    var endReason = charReader["EndReason"]?.ToString() ?? "NULL";
    var winnerTeam = charReader["WinnerTeam"]?.ToString() ?? "NULL";
    Console.WriteLine($"{charReader["RoundNumber"],-5} | {endReason,-9} | {winnerTeam,-6} | {hasEndTime,-10} | {charReader["KillCount"],-5} | {charReader["WeaponFireCount"]}");
}
