using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var connectionString = "Server=localhost;Database=CS2DemoParser;Trusted_Connection=true;";
        
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        
        var query = @"
            SELECT TOP 30 
                r.RoundNumber, 
                r.StartTime, 
                r.EndTime, 
                r.WinnerTeam, 
                r.EndReason, 
                d.DemoSource, 
                d.FileName,
                d.Id as DemoId
            FROM Rounds r 
            INNER JOIN Matches m ON r.MatchId = m.Id 
            INNER JOIN DemoFiles d ON m.DemoFileId = d.Id 
            WHERE d.DemoSource IS NOT NULL 
            ORDER BY d.Id, r.RoundNumber";
        
        using var command = new SqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();
        
        Console.WriteLine("DemoId | DemoSource | RoundNumber | StartTime | EndTime | WinnerTeam | EndReason | FileName");
        Console.WriteLine(new string('-', 120));
        
        while (await reader.ReadAsync())
        {
            Console.WriteLine($"{reader["DemoId"],-6} | {reader["DemoSource"],-10} | {reader["RoundNumber"],-11} | {reader["StartTime"],-9:F1} | {reader["EndTime"],-7:F1} | {reader["WinnerTeam"],-10} | {reader["EndReason"],-9} | {reader["FileName"]}");
        }
    }
}