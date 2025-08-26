-- Database Performance Optimization Indexes
-- Run these commands on your SQL Server database to improve report query performance

-- Index for Players table lookups
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Players_DemoFileId_PlayerName' AND object_id = OBJECT_ID('Players'))
CREATE NONCLUSTERED INDEX IX_Players_DemoFileId_PlayerName 
ON Players (DemoFileId, PlayerName) 
INCLUDE (Team);

-- Index for Kills table - KillerId lookups
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Kills_KillerId' AND object_id = OBJECT_ID('Kills'))
CREATE NONCLUSTERED INDEX IX_Kills_KillerId 
ON Kills (KillerId) 
INCLUDE (IsHeadshot, VictimId);

-- Index for Kills table - VictimId lookups
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Kills_VictimId' AND object_id = OBJECT_ID('Kills'))
CREATE NONCLUSTERED INDEX IX_Kills_VictimId 
ON Kills (VictimId);

-- Index for WeaponFires table
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_WeaponFires_PlayerId' AND object_id = OBJECT_ID('WeaponFires'))
CREATE NONCLUSTERED INDEX IX_WeaponFires_PlayerId 
ON WeaponFires (PlayerId);

-- Index for Damages table
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Damages_AttackerId' AND object_id = OBJECT_ID('Damages'))
CREATE NONCLUSTERED INDEX IX_Damages_AttackerId 
ON Damages (AttackerId) 
INCLUDE (DamageAmount);

-- Index for DemoFiles table
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_DemoFiles_ParsedAt_MapName' AND object_id = OBJECT_ID('DemoFiles'))
CREATE NONCLUSTERED INDEX IX_DemoFiles_ParsedAt_MapName 
ON DemoFiles (ParsedAt, MapName) 
INCLUDE (FileName);

-- Composite index for date range filtering
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_DemoFiles_Dates' AND object_id = OBJECT_ID('DemoFiles'))
CREATE NONCLUSTERED INDEX IX_DemoFiles_Dates 
ON DemoFiles (ParsedAt, MapName, Id);

-- For economy and rounds reports
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Rounds_MatchId' AND object_id = OBJECT_ID('Rounds'))
CREATE NONCLUSTERED INDEX IX_Rounds_MatchId 
ON Rounds (MatchId) 
INCLUDE (RoundNumber, WinnerTeam, EndReason);

-- Index for matches
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Matches_DemoFileId' AND object_id = OBJECT_ID('Matches'))
CREATE NONCLUSTERED INDEX IX_Matches_DemoFileId 
ON Matches (DemoFileId);

PRINT 'Performance indexes created successfully. Player stats report should now run much faster.'