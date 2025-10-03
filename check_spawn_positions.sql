-- Check if player positions are being tracked at round start (spawn positions)
-- Query 1: Get position counts per round for a specific demo
SELECT
    r.RoundNumber,
    COUNT(pp.Id) as PositionCount,
    MIN(pp.Tick) as FirstTick,
    MAX(pp.Tick) as LastTick,
    MIN(pp.GameTime) as StartTime,
    MAX(pp.GameTime) as EndTime
FROM Rounds r
LEFT JOIN PlayerPositions pp ON r.DemoFileId = pp.DemoFileId
WHERE r.DemoFileId = (SELECT TOP 1 Id FROM DemoFiles ORDER BY Id DESC)
GROUP BY r.RoundNumber
ORDER BY r.RoundNumber;

-- Query 2: Get early-round positions (first 5 seconds) to see spawn locations
SELECT TOP 20
    r.RoundNumber,
    p.PlayerName,
    pp.Tick,
    pp.GameTime,
    pp.PositionX,
    pp.PositionY,
    pp.PositionZ
FROM PlayerPositions pp
INNER JOIN Players p ON pp.PlayerId = p.Id
INNER JOIN Rounds r ON r.DemoFileId = pp.DemoFileId
WHERE pp.DemoFileId = (SELECT TOP 1 Id FROM DemoFiles ORDER BY Id DESC)
    AND pp.GameTime BETWEEN r.StartTime AND DATEADD(second, 5, r.StartTime)
    AND r.RoundNumber = 1  -- Check first live round
ORDER BY pp.Tick;

-- Query 3: Check position variance at round start vs mid-round
SELECT
    r.RoundNumber,
    'Round Start (0-5s)' as Phase,
    COUNT(DISTINCT CONCAT(CAST(pp.PositionX AS VARCHAR), ',', CAST(pp.PositionY AS VARCHAR))) as UniquePositions
FROM PlayerPositions pp
INNER JOIN Rounds r ON r.DemoFileId = pp.DemoFileId
WHERE pp.DemoFileId = (SELECT TOP 1 Id FROM DemoFiles ORDER BY Id DESC)
    AND pp.GameTime BETWEEN r.StartTime AND DATEADD(second, 5, r.StartTime)
GROUP BY r.RoundNumber

UNION ALL

SELECT
    r.RoundNumber,
    'Mid Round (30-35s)' as Phase,
    COUNT(DISTINCT CONCAT(CAST(pp.PositionX AS VARCHAR), ',', CAST(pp.PositionY AS VARCHAR))) as UniquePositions
FROM PlayerPositions pp
INNER JOIN Rounds r ON r.DemoFileId = pp.DemoFileId
WHERE pp.DemoFileId = (SELECT TOP 1 Id FROM DemoFiles ORDER BY Id DESC)
    AND pp.GameTime BETWEEN DATEADD(second, 30, r.StartTime) AND DATEADD(second, 35, r.StartTime)
GROUP BY r.RoundNumber
ORDER BY RoundNumber, Phase;
