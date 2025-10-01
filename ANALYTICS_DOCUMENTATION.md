# CS2DemoParserWeb Analytics Endpoints Documentation

## Table of Contents

1. [Overview](#overview)
2. [Common Query Parameters](#common-query-parameters)
3. [Endpoint Listings](#endpoint-listings)
   - [Clutch Analysis](#1-clutch-analysis)
   - [Trade Kill Analysis](#2-trade-kill-analysis)
   - [First Kill Impact](#3-first-kill-impact)
   - [Economic Intelligence](#4-economic-intelligence)
   - [Position Analysis](#5-position-analysis)
   - [Enhanced Trade Kill Optimization](#6-enhanced-trade-kill-optimization)
   - [Weapon Mastery Analytics](#7-weapon-mastery-analytics)
   - [Grenade Impact Quantification](#8-grenade-impact-quantification)
   - [First Blood Impact Analysis](#9-first-blood-impact-analysis)
   - [Economic Efficiency Analysis](#10-economic-efficiency-analysis)
   - [Round Momentum Tracking](#11-round-momentum-tracking)
   - [Positioning Intelligence](#12-positioning-intelligence)
   - [Team Coordination Metrics](#13-team-coordination-metrics)
   - [Performance Consistency Profiling](#14-performance-consistency-profiling)
   - [Damage Efficiency Analytics](#15-damage-efficiency-analytics)
   - [Comprehensive Clutch Analysis](#16-comprehensive-clutch-analysis)
   - [Positioning Analysis (Simple)](#17-positioning-analysis-simple)
   - [Weapon Intelligence](#18-weapon-intelligence)
   - [Circumstantial Combat](#19-circumstantial-combat)
   - [Team Coordination](#20-team-coordination)
   - [Pressure Metrics](#21-pressure-metrics)
   - [Database Diagnostic](#22-database-diagnostic)
   - [Economy Intelligence Dashboard](#23-economy-intelligence-dashboard)
   - [Advanced Player Performance](#24-advanced-player-performance)
   - [Player Inventory](#25-player-inventory)
   - [Master Analytics Dashboard](#26-master-analytics-dashboard)
   - [Situation Analysis](#27-situation-analysis)
   - [Economy Intelligence Enhanced](#28-economy-intelligence-enhanced)
   - [Movement & Positioning](#29-movement--positioning)
   - [Timing & Tempo](#30-timing--tempo)
   - [Weapon Mastery](#31-weapon-mastery)
   - [Match Flow](#32-match-flow)
   - [Performance Trends](#33-performance-trends)
   - [Team Dynamics](#34-team-dynamics)

---

## Overview

The **AdvancedAnalyticsController** provides comprehensive analytics endpoints for Counter-Strike 2 demo analysis. All endpoints support CSV export via the `format=csv` query parameter.

**Base Route:** `/api/advanced-analytics`

---

## Common Query Parameters

All endpoints (except `database-diagnostic`) support the following optional query parameters:

| Parameter | Type | Description |
|-----------|------|-------------|
| `DemoId` | int | Filter by specific demo file ID |
| `MapName` | string | Filter by map name (e.g., "de_dust2") |
| `PlayerName` | string | Filter by specific player name |
| `Team` | string | Filter by team ("CT" or "TERRORIST") |
| `StartDate` | DateTime | Filter demos parsed after this date |
| `EndDate` | DateTime | Filter demos parsed before this date |
| `RoundNumber` | int | Filter by specific round number |
| `Format` | string | Output format ("csv" for CSV export) |

---

## Endpoint Listings

### 1. Clutch Analysis

**Route:** `GET /api/advanced-analytics/clutch-analysis`

**Purpose:** Analyzes 1vX clutch situations and calculates success rates.

**Data Sources:**
- `Kills` - Kill events
- `Players` - Player information
- `Rounds` - Round outcomes
- `DemoFiles` - Demo metadata

**Key Metrics:**
- `ClutchAttempts` - Total clutch situations encountered
- `ClutchWins` - Successful clutches
- `ClutchSuccessRate` - Win percentage in clutch situations
- `ClutchType` - Classification (1v1, 1v2, 1v3, 1v4, 1v5)
- `DemosPlayed` - Number of distinct demos

**Calculation Method:**
1. **LastKillsPerRound CTE:** Identifies last 4 kills per round ordered by game time
2. **PotentialClutches CTE:** Groups consecutive final kills by same player
3. Categorizes clutch type based on kill count (2+ kills = clutch)
4. Calculates success rate by comparing killer team with winner team

**SQL Key Logic:**
```sql
-- Identifies clutch scenarios from final kills
HAVING COUNT(*) >= 2 -- At least 2 kills in final moments
```

---

### 2. Trade Kill Analysis

**Route:** `GET /api/advanced-analytics/trade-kill-analysis`

**Purpose:** Analyzes team's ability to quickly trade frags after losing a teammate.

**Data Sources:**
- `Kills` (k1, k2) - Self-joined for trade detection
- `Players` - Player and team info
- `Rounds` - Round context
- `DemoFiles` - Demo metadata

**Key Metrics:**
- `TotalTrades` - Count of successful trade kills
- `AvgTradeTimeSeconds` - Average time to revenge kill
- `FastestTradeSeconds` / `SlowestTradeSeconds` - Time bounds
- `AvgTradeDistance` - Distance between kills
- `FastTrades` - Trades within 2 seconds
- `FastTradePercentage` - Percentage of fast trades

**Calculation Method:**
1. Self-join `Kills` table with 5-second window constraint
2. Match kills where:
   - Second kill targets the initial killer
   - Teams are opposite
   - Victim's team matches trade killer's team
3. Calculate distance using Pythagorean theorem on positions
4. Aggregate trade timing statistics

**SQL Key Logic:**
```sql
AND k2.GameTime > k1.GameTime
AND k2.GameTime <= k1.GameTime + 5.0 -- Trades within 5 seconds
AND trade_victim.Id = initial_killer.Id -- Revenge kill
```

---

### 3. First Kill Impact

**Route:** `GET /api/advanced-analytics/first-kill-impact`

**Purpose:** Correlates first blood kills with round win probability.

**Data Sources:**
- `Kills` - Kill events
- `Players` - Player information
- `Rounds` - Round winners
- `DemoFiles` - Demo metadata

**Key Metrics:**
- `TotalFirstKills` - Count of first kills
- `RoundsWonAfterFirstKill` - Wins after getting first kill
- `FirstKillWinPercentage` - Correlation with round victory
- `HeadshotFirstKills` / `HeadshotPercentage` - Precision metrics
- `FirstKillType` - Categorization (Headshot, AWP Pick, Long Range, Standard)
- `AvgFirstKillTime` - Timing of first bloods

**Calculation Method:**
1. **RoundFirstKills CTE:** Identifies all kills with ordering
2. **FirstKills CTE:** Filters to KillOrder = 1 (first kill per round)
3. Categorizes first kill types based on weapon and conditions
4. Correlates with round outcomes

**SQL Key Logic:**
```sql
ROW_NUMBER() OVER (PARTITION BY k.RoundId ORDER BY k.GameTime ASC) as KillOrder
WHERE KillOrder = 1 -- First kill only
```

---

### 4. Economic Intelligence

**Route:** `GET /api/advanced-analytics/economic-intelligence`

**Purpose:** Analyzes player performance across different economic states (eco, force buy, full buy).

**Data Sources:**
- `Players` - Player data
- `PlayerRoundStats` - Round-level stats
- `Rounds` - Round outcomes
- `DemoFiles` - Demo metadata
- `Kills` - Kill events

**Key Metrics:**
- `EconomyType` - Weapon category (High_Value_Rifle, Sniper_Investment, Pistol_Economy, SMG_Force)
- `RoundType` - Phase classification (Pistol_Round, Anti_Eco_Round, Buy_Round, Late_Round)
- `KillRateWithEconomyType` - Kill success rate per economy state
- `WinRateWithEconomyType` - Win rate correlation
- `EconomicIntelligenceScore` - 0-100 score based on performance

**Calculation Method:**
1. Joins player stats with kills to analyze weapon usage
2. Categorizes economy based on weapon types from kills
3. Classifies round types based on round numbers
4. Scores economic intelligence using tiered thresholds

**SQL Key Logic:**
```sql
CASE
    WHEN EconomyType = 'High_Value_Rifle' AND WinRate >= 70 THEN 95
    WHEN EconomyType = 'Pistol_Economy' AND KillRate >= 40 THEN 85
    ...
END as EconomicIntelligenceScore
```

---

### 5. Position Analysis

**Route:** `GET /api/advanced-analytics/position-analysis`

**Purpose:** Identifies death hotspots and dangerous areas by map location.

**Data Sources:**
- `Kills` - Kill positions
- `Players` - Victim information
- `DemoFiles` - Map context

**Key Metrics:**
- `TotalDeaths` - Deaths in specific area
- `AvgKillDistance` - Average kill range in area
- `HeadshotDeaths` / `HeadshotPercentage` - Precision in area
- `MapArea` - Classified zone (A Site, B Site, Mid, etc.)
- `CommonWeapons` - Aggregated weapon list
- `DeathSpreadX/Y` - Position variance (clustering)

**Calculation Method:**
1. **DeathHotspots CTE:** Maps victim positions to map areas
2. Uses coordinate-based CASE statements for area classification
3. Aggregates STRING_AGG for weapon lists
4. Calculates position spread using STDEV functions

**SQL Key Logic:**
```sql
CASE
    WHEN d.MapName = 'de_dust2' THEN
        CASE
            WHEN k.VictimPositionY > 1000 THEN 'A Site'
            WHEN k.VictimPositionY < -1000 THEN 'B Site'
            ...
        END
END as MapArea
```

---

### 6. Enhanced Trade Kill Optimization

**Route:** `GET /api/advanced-analytics/enhanced-trade-kill-optimization`

**Purpose:** Advanced trade kill analysis including positioning, timing, chains, and missed opportunities.

**Data Sources:**
- `Kills` (k1, k2) - Trade detection
- `Players` - Player/team data
- `Rounds` - Round context
- `DemoFiles` - Map/demo info

**Key Metrics:**
- `EffectiveTradePercentage` - Instant + Fast trades percentage
- `AvgTradeChainLength` - Length of trade sequences
- `TradeChains` - Multi-kill trade sequences
- `CloseRangeTrades` / `LongRangeTrades` - Distance breakdown
- `StaticTrades` / `MobileTrades` - Movement analysis
- `TradeQuality` - Classification (Instant, Fast, Slow, Too Late)

**Calculation Method:**
1. **TradeKillAnalysis CTE:** Basic trade detection with 8-second window
2. **TradeChainAnalysis CTE:** Self-joins to detect sequential trades
3. **FailedTradeOpportunities CTE:** NOT EXISTS query for missed trades
4. Calculates trader movement distance between positions
5. Maps positions to areas for spatial analysis

**SQL Key Logic:**
```sql
-- Trade quality categorization
CASE
    WHEN (k2.GameTime - k1.GameTime) <= 1.0 THEN 'Instant'
    WHEN (k2.GameTime - k1.GameTime) <= 3.0 THEN 'Fast'
    WHEN (k2.GameTime - k1.GameTime) <= 5.0 THEN 'Slow'
    ELSE 'Too Late'
END as TradeQuality
```

---

### 7. Weapon Mastery Analytics

**Route:** `GET /api/advanced-analytics/weapon-mastery-analytics`

**Purpose:** Detailed weapon-specific performance including accuracy, range effectiveness, and switching patterns.

**Data Sources:**
- `Kills` - Kill data with weapons
- `Players` - Player information
- `Rounds` - Round context
- `DemoFiles` - Demo metadata
- `WeaponFires` - Shot accuracy data
- `Equipment` - Weapon switching patterns

**Key Metrics:**
- `TotalKills` / `HeadshotPercentage` - Kill statistics
- `CloseRangeKills`, `MediumRangeKills`, `LongRangeKills` - Range distribution
- `WallbangKills`, `NoScopeKills` - Special kills
- `EstimatedAccuracyPercentage` - Kill-to-shot ratio
- `AvgSwitchSpeed` - Weapon switching proficiency
- `DistanceConsistency` - STDEV of kill distances

**Calculation Method:**
1. **WeaponPerformanceData CTE:** Categorizes weapons and ranges
2. **WeaponFireData CTE:** Aggregates shot counts per weapon
3. **WeaponSwitchingPatterns CTE:** Analyzes equipment changes within 1 second
4. Joins all CTEs to calculate comprehensive weapon stats
5. Estimates accuracy as kills / average shots

**SQL Key Logic:**
```sql
-- Accuracy estimation from shot data
CASE
    WHEN AVG(TotalShots) > 0
    THEN COUNT(*) * 100.0 / AVG(TotalShots)
    ELSE NULL
END as EstimatedAccuracyPercentage
```

---

### 8. Grenade Impact Quantification

**Route:** `GET /api/advanced-analytics/grenade-impact-quantification`

**Purpose:** Analyzes utility effectiveness including flash duration, smoke timing, HE damage, and team coordination.

**Data Sources:**
- `Grenades` - Grenade throws
- `Players` - Player information
- `Rounds` - Round outcomes
- `DemoFiles` - Demo metadata
- `FlashEvents` - Flash effectiveness
- `Grenades` (self-joined) - Utility combos

**Key Metrics:**
- `TotalGrenades` - Grenade count
- `AvgThrowDistance` - Throw range
- `GrenadesInWonRounds` / `WinRateWithGrenades` - Utility impact on wins
- `AvgFlashDuration` - Flash effectiveness
- `EnemyFlashes` / `TeamFlashes` - Flash targeting
- `FullBlinds` - Full-blind count
- `UtilityCombos` - Coordinated utility (within 3 seconds)

**Calculation Method:**
1. **GrenadeEffectivenessData CTE:** Base grenade data with positions
2. **FlashEffectiveness CTE:** Flash duration and blind levels
3. **UtilityComboDetection CTE:** Detects grenades thrown within 3 seconds (384 ticks)
4. Aggregates utility metrics per player/area
5. Calculates win correlation with grenade usage

**SQL Key Logic:**
```sql
-- Detect coordinated utility
ABS(g1.ThrowTick - g2.ThrowTick) <= 384 -- Within ~3 seconds
WHERE p1.Team = p2.Team -- Same team coordination
```

---

### 9. First Blood Impact Analysis

**Route:** `GET /api/advanced-analytics/first-blood-impact-analysis`

**Purpose:** Deep analysis of first kill impact including location heatmaps, weapon effectiveness, and timing correlation.

**Data Sources:**
- `Kills` - Kill events (filtered by `IsFirstKill = 1`)
- `Players` - Player data
- `Rounds` - Round outcomes
- `DemoFiles` - Map context

**Key Metrics:**
- `TotalFirstKills` - First kill count
- `FirstKillWinPercentage` - Round win correlation
- `RifleFirstKills`, `SniperFirstKills`, `PistolFirstKills` - Weapon breakdown
- `HeadshotPercentage` - Precision metric
- `WallbangFirstKills`, `SmokeFirstKills` - Special first kills
- `FirstKillArea` / `FirstKillTiming` - Spatial and temporal classification
- `ViewAngleMagnitude` - Crosshair placement analysis

**Calculation Method:**
1. Uses `IsFirstKill = 1` flag to filter first bloods
2. Maps positions to areas per map
3. Categorizes timing (Early 0-15s, Mid 15-45s, Late 45-75s, Very Late 75s+)
4. Calculates view angle magnitude for crosshair analysis
5. Aggregates positioning statistics (avg positions, consistency)

**SQL Key Logic:**
```sql
WHERE k.IsFirstKill = 1
-- Timing categorization
CASE
    WHEN k.GameTime <= 15.0 THEN 'Early (0-15s)'
    WHEN k.GameTime <= 45.0 THEN 'Mid (15-45s)'
    ...
END as FirstKillTiming
```

---

### 10. Economic Efficiency Analysis

**Route:** `GET /api/advanced-analytics/economic-efficiency-analysis`

**Purpose:** Deep analysis of equipment ROI, force buy patterns, save optimization, and economic momentum.

**Data Sources:**
- `PlayerRoundStats` - Performance and equipment value
- `Players` - Player information
- `Rounds` - Round outcomes
- `DemoFiles` - Demo metadata

**Key Metrics:**
- `EquipmentROI` - Return on investment ((Kills*1000 + Assists*300 + Damage) / EquipmentValue)
- `EconomicState` - Classification (Full Buy, Half Buy, Force Buy, Eco)
- `BuyStrategy` - Context-aware strategy (Anti-Eco Force, Momentum Force)
- `AvgEquipmentROI` / `MaxEquipmentROI` - ROI statistics
- `KillsPerEquippedRound`, `DamagePerDollar` - Efficiency metrics
- `AntiEcoForceWinRate`, `EcoRoundWinRate` - Strategy success rates
- `WinRateAfterWin`, `WinRateAfterLoss` - Momentum tracking

**Calculation Method:**
1. **EconomicRoundData CTE:** Gathers equipment value and performance
2. Uses LAG() to track previous round outcomes for momentum
3. **EconomicEfficiency CTE:** Calculates ROI and strategy classification
4. Aggregates efficiency metrics per player/state/strategy
5. Analyzes post-win and post-loss performance

**SQL Key Logic:**
```sql
-- ROI Calculation
CASE
    WHEN EquipmentValue > 0 THEN
        (Kills * 1000 + Assists * 300 + Damage) / EquipmentValue
    ELSE 0
END as EquipmentROI

-- Momentum tracking with LAG
LAG(CASE WHEN Team = WinnerTeam THEN 1 ELSE 0 END) OVER
    (PARTITION BY PlayerId ORDER BY RoundNumber) as PreviousRoundWon
```

---

### 11. Round Momentum Tracking

**Route:** `GET /api/advanced-analytics/round-momentum-tracking`

**Purpose:** Analyzes streak patterns, comeback potential, momentum shifts, and force reset identification.

**Data Sources:**
- `Rounds` - Round sequences and outcomes
- `DemoFiles` - Demo context
- `Kills` (subquery) - First kill timing

**Key Metrics:**
- `AvgStreakLength` / `MaxStreak` - Winning streaks
- `TotalMomentumShifts` / `MomentumShiftPercentage` - Momentum changes
- `ComebackRounds` / `ComebackPercentage` - Reversals after first blood loss
- `FirstKillAdvantagePercentage` - First blood win correlation
- `ForceResetRounds` - Breaking losing streaks
- `MomentumBuildRounds` - Consecutive wins
- `WinnerKillDominance` - Kill share of winning team

**Calculation Method:**
1. **RoundSequenceData CTE:** Uses LAG() for previous 3 round winners
2. Subqueries identify first kill team per round
3. **MomentumStreaks CTE:** Classifies momentum patterns
4. Calculates streak lengths with ROW_NUMBER()
5. Identifies force resets (no wins in last 3 rounds) and momentum builds

**SQL Key Logic:**
```sql
-- Momentum shift detection
CASE WHEN LAG(WinnerTeam) OVER (...) != WinnerTeam THEN 1 ELSE 0 END as MomentumShift

-- Force reset identification
CASE
    WHEN PreviousWinner != WinnerTeam AND
         TwoRoundsAgoWinner != WinnerTeam AND
         ThreeRoundsAgoWinner != WinnerTeam THEN 'Force Reset'
END
```

---

### 12. Positioning Intelligence

**Route:** `GET /api/advanced-analytics/positioning-intelligence`

**Purpose:** Death probability heatmaps, movement patterns, angle holding effectiveness, and site control.

**Data Sources:**
- `Kills` - Death positions
- `Players` - Player information
- `Rounds` - Round context
- `DemoFiles` - Map data
- `EnhancedPlayerPositions` - Movement data

**Key Metrics:**
- `TotalDeaths` / `DeathProbabilityPercentage` - Death risk by area
- `AvgDeathDistance` / `DeathDistanceVariation` - Kill distance stats
- `AK47Deaths`, `M4Deaths`, `AWPDeaths` - Weapon breakdown
- `AvgMovementSpeedAtDeath` - Movement correlation
- `StationaryPercentage` - Static vs mobile deaths
- `LongAngleDeathPercentage` - Angle holding risks
- `TerroristDeathPercentage` - Site control indicators

**Calculation Method:**
1. **DeathPositionAnalysis CTE:** Maps death positions to areas and analyzes angles
2. **MovementPatternAnalysis CTE:** Calculates speed and coverage from position data
3. Joins movement patterns with death locations
4. Calculates death probability as percentage of total deaths on map
5. Analyzes weapon effectiveness per position

**SQL Key Logic:**
```sql
-- Death probability calculation
CAST(COUNT(*) AS FLOAT) /
    (SELECT COUNT(*) FROM DeathPositionAnalysis sub WHERE sub.MapName = dpa.MapName) * 100
    as DeathProbabilityPercentage

-- Movement analysis
AVG(SQRT(POWER(VelocityX, 2) + POWER(VelocityY, 2))) as AvgSpeed
```

---

### 13. Team Coordination Metrics

**Route:** `GET /api/advanced-analytics/team-coordination-metrics`

**Purpose:** Analyzes rotation timing, stack effectiveness, spread formation, and synchronized utility usage.

**Data Sources:**
- `EnhancedPlayerPositions` - Position/velocity data
- `Players` - Player information
- `Rounds` - Round context
- `DemoFiles` - Demo metadata
- `Grenades` - Utility timing

**Key Metrics:**
- `TotalRotations` / `EarlyRotationPercentage` - Rotation timing
- `AvgStackSize`, `FullStackCount`, `TripleStackCount` - Stack analysis
- `AvgStackSpread` - Stack tightness (distance)
- `AStackPercentage`, `BStackPercentage`, `MidStackPercentage` - Stack distribution
- `TightStackPercentage` - Close formations
- `SynchronizedUtilityCount` - Coordinated grenades
- `FlashCombos`, `SmokeCombos`, `HECombos` - Utility coordination
- `CoordinationScore` - Weighted score (0-100)

**Calculation Method:**
1. **RotationAnalysis CTE:** Tracks area changes using LAG()
2. **StackFormationAnalysis CTE:** Identifies player groupings in areas
3. Calculates stack spread using position min/max
4. **UtilityCoordinationAnalysis CTE:** Detects grenades within 3-second windows
5. Weighted coordination score: Tight Stacks (30%) + Early Rotations (25%) + Sync Utility (45%)

**SQL Key Logic:**
```sql
-- Area rotation detection
LAG(CurrentArea, 10) OVER (PARTITION BY PlayerId, RoundId ORDER BY Tick) as PreviousArea
WHERE CurrentArea != PreviousArea

-- Stack spread calculation
SQRT(POWER(MAX(PositionX) - MIN(PositionX), 2) +
     POWER(MAX(PositionY) - MIN(PositionY), 2)) as StackSpread
```

---

### 14. Performance Consistency Profiling

**Route:** `GET /api/advanced-analytics/performance-consistency-profiling`

**Purpose:** Tilt detection, variance tracking, and adaptation analysis for performance consistency.

**Data Sources:**
- `PlayerRoundStats` - Round-level performance
- `Players` - Player information
- `Rounds` - Round context
- `DemoFiles` - Demo metadata

**Key Metrics:**
- `AverageKills`, `AverageDeaths`, `AverageAssists`, `AverageDamage` - Mean stats
- `AverageRating` - Overall performance rating
- `KillVariance`, `DamageVariance` - STDEV consistency measures
- `BestKillRound`, `WorstKillRound` - Performance bounds
- `FullBuyKills`, `EcoKills` - Economic context performance
- `ConsistencyLevel` - Classification (Very Consistent to Very Inconsistent)
- `ConsistencyScore` - 0-100 score based on variance

**Calculation Method:**
1. Aggregates player statistics across rounds
2. Calculates STDEV for kills and damage
3. Separate averages for full buy vs eco rounds
4. Scores consistency based on kill variance thresholds

**SQL Key Logic:**
```sql
STDEV(CAST(Kills AS FLOAT)) as KillVariance

-- Consistency scoring
CASE
    WHEN KillVariance <= 0.8 THEN 'Very Consistent' -- Score: 90
    WHEN KillVariance <= 1.2 THEN 'Consistent' -- Score: 75
    WHEN KillVariance <= 1.8 THEN 'Moderate' -- Score: 60
    ...
END
```

---

### 15. Damage Efficiency Analytics

**Route:** `GET /api/advanced-analytics/damage-efficiency-analytics`

**Purpose:** Comprehensive damage waste, armor penetration, finishing ability, and multi-target tracking.

**Data Sources:**
- `Kills` - Damage events
- `Players` - Player information
- `Rounds` - Round context
- `DemoFiles` - Demo metadata

**Key Metrics:**
- `DamageWastePercentage` - Overkill damage percentage
- `FinishingPercentage` - Kill success rate
- `DamagePerKill` - Damage required per kill
- `ArmorPenDamage` vs `UnarmoredDamage` - Armor effectiveness
- `HeadshotPercentage` - Precision metric
- `MultiTargetRounds` / `AvgTargetsPerMultiKill` - Multi-kill capability
- `DamageEfficiencyScore`, `OverallEfficiencyScore` - Composite scores
- `PerformanceCategory` - Classification (Elite Sniper, Excellent Rifle Control, etc.)

**Calculation Method:**
1. **DamageBreakdownAnalysis CTE:** Calculates effective vs wasted damage
2. Applies armor penetration modifiers (AK 77%, M4 70%, AWP 100%)
3. **MultiTargetAnalysis CTE:** Detects multi-kill windows (5 seconds)
4. **WasteIdentificationAnalysis CTE:** Aggregates waste metrics
5. Overall score: Finishing (40%) + Efficiency (30%) + Headshot (20%) + Multi-target bonus (10%)

**SQL Key Logic:**
```sql
-- Effective damage calculation
CASE WHEN VictimHealth <= Damage THEN VictimHealth ELSE Damage END as EffectiveDamage
-- Wasted damage
CASE WHEN VictimHealth <= Damage THEN 0 ELSE Damage - VictimHealth END as WastedDamage

-- Armor penetration
CASE
    WHEN VictimArmor > 0 AND Weapon LIKE '%ak47%' THEN Damage * 0.77
    WHEN VictimArmor > 0 AND Weapon LIKE '%m4%' THEN Damage * 0.70
    ...
END
```

---

### 16. Comprehensive Clutch Analysis

**Route:** `GET /api/advanced-analytics/comprehensive-clutch-analysis`

**Purpose:** Deep clutch analysis including positioning, health/armor states, economic context, and multi-clutch performance.

**Data Sources:**
- `Kills` - Clutch kill events
- `Players` - Player information
- `Rounds` - Round outcomes
- `DemoFiles` - Demo metadata
- `PlayerRoundStats` - Equipment and stats

**Key Metrics:**
- `TotalClutchAttempts` / `TotalClutchWins` - Overall clutch performance
- `Clutch1v2`, `Clutch1v3`, `Clutch1v4Plus` - Success rates by clutch size
- `HealthyClutchSuccessRate` vs `LowHealthClutchSuccessRate` - Health impact
- `FullBuyClutchSuccessRate` vs `EcoClutchSuccessRate` - Economic correlation
- `RifleClutches`, `AwpClutches`, `PistolClutches` - Weapon preferences
- `AvgClutchEquipmentValue` - Investment analysis
- `AvgClutchKillDistance` - Engagement range

**Calculation Method:**
1. **ClutchSetupAnalysis CTE:** Uses multi-kill rounds as proxy for clutches
2. Categorizes health conditions (Healthy, Moderate, Low, Critical)
3. Classifies equipment levels (Full Buy, Partial, Light, Eco)
4. **MultiClutchPlayers CTE:** Aggregates clutch performance
5. Analyzes success rates across different variables

**SQL Key Logic:**
```sql
-- Clutch identification (using kills as proxy)
WHERE prs2.Kills >= 2 -- Multi-kill scenarios

-- Equipment categorization
CASE
    WHEN EquipmentValue >= 4000 THEN 'Full Buy'
    WHEN EquipmentValue >= 2000 THEN 'Partial Buy'
    ...
END
```

---

### 17. Positioning Analysis (Simple)

**Route:** `GET /api/advanced-analytics/positioning-analysis`

**Purpose:** Simplified performance analysis by game phase (first half vs second half).

**Data Sources:**
- `Players` - Player information
- `PlayerRoundStats` - Round stats
- `Rounds` - Round context
- `DemoFiles` - Demo metadata

**Key Metrics:**
- `RoundsPlayed`, `RoundsWon`, `WinPercentage` - Win rate by position
- `TotalKills`, `TotalDeaths`, `TotalAssists`, `TotalDamage` - Aggregate stats
- `KDRatio` - Kill/death ratio
- `AvgDamagePerRound` - Consistent damage output
- `RoundsSurvived`, `SurvivalRate` - Survival metrics
- `PositionContext` - First_Half vs Second_Half

**Calculation Method:**
1. Simple round number check for half classification (<=15 = First_Half)
2. Aggregates performance by player/team/map/half
3. Calculates ratios and percentages

**SQL Key Logic:**
```sql
CASE
    WHEN RoundNumber <= 15 THEN 'First_Half'
    ELSE 'Second_Half'
END as PositionContext
```

---

### 18. Weapon Intelligence

**Route:** `GET /api/advanced-analytics/weapon-intelligence`

**Purpose:** Weapon-specific performance including headshot rates, distance effectiveness, and proficiency scoring.

**Data Sources:**
- `Players` - Player information
- `PlayerRoundStats` - Round stats
- `Rounds` - Round context
- `DemoFiles` - Demo metadata
- `Kills` - Weapon kill data

**Key Metrics:**
- `KillsWithWeapon` - Kill count per weapon
- `HeadshotRate` - Headshot percentage
- `AvgKillDistance`, `MinKillDistance`, `MaxKillDistance` - Range statistics
- `WinRateWithWeapon` - Round win correlation
- `AvgRoundDamage` - Overall damage contribution
- `WeaponProficiencyScore` - 0-100 score based on performance
- `WeaponClass` - Classification (Rifle, Sniper, Pistol, SMG)

**Calculation Method:**
1. Joins kills with player round stats
2. Categorizes weapons into classes
3. Aggregates kill statistics per weapon
4. Scores proficiency using tiered thresholds

**SQL Key Logic:**
```sql
-- Weapon proficiency scoring
CASE
    WHEN HeadshotRate >= 60 AND KillCount >= 10 THEN 95
    WHEN HeadshotRate >= 40 AND KillCount >= 5 THEN 80
    WHEN WinRate >= 70 THEN 75
    ...
END as WeaponProficiencyScore
```

---

### 19. Circumstantial Combat

**Route:** `GET /api/advanced-analytics/circumstantial-combat`

**Purpose:** Performance analysis under special combat conditions (precision, range, weapon-specific).

**Data Sources:**
- `Players` - Player information
- `PlayerRoundStats` - Round stats
- `Rounds` - Round context
- `DemoFiles` - Demo metadata
- `Kills` - Combat conditions

**Key Metrics:**
- `EngagementsInCondition` - Count of combat scenarios
- `KillsInCondition` - Successful kills
- `ConditionKillPercentage` - Success rate
- `WeaponsUsedInCondition` - Weapon variety
- `HeadshotRateInCondition` - Precision metric
- `LongRangeKills` / `CloseRangeKills` - Distance breakdown
- `CircumstantialMasteryScore` - Scaled performance score (higher for difficult conditions)

**Calculation Method:**
1. Categorizes combat conditions based on distance, headshot, weapon
2. Aggregates performance per condition
3. Applies multipliers for difficult conditions (Precision_LongRange = 2x)

**SQL Key Logic:**
```sql
-- Condition classification
CASE
    WHEN IsHeadshot = 1 AND Distance > 800 THEN 'Precision_LongRange'
    WHEN Distance > 1200 THEN 'Long_Range'
    ...
END as CombatCondition

-- Mastery scoring with multipliers
CASE
    WHEN CombatCondition = 'Precision_LongRange' THEN KillRate * 2.0
    WHEN CombatCondition LIKE '%Precision%' THEN KillRate * 1.5
    ...
END
```

---

### 20. Team Coordination

**Route:** `GET /api/advanced-analytics/team-coordination`

**Purpose:** Team kill coordination, flash teamwork, utility support, and coordinated execution.

**Data Sources:**
- `Players` (p) - Player information
- `PlayerRoundStats` (prs) - Player stats
- `Rounds` - Round context
- `DemoFiles` - Demo metadata
- `Players` (teammate) - Teammate information
- `Kills` - Kill timing for coordination

**Key Metrics:**
- `TeammatesPlayedWith` - Unique teammate count
- `TotalKills` - Player kill count
- `CoordinatedKills` - Kills within 2 seconds of teammate
- `SupportKills` - Kills within 5 seconds
- `AvgKillCoordinationTime` - Average time between teammate kills
- `ComplementaryWeaponKills` - Different weapons used together
- `CoordinationEffectivenessScore` - Combined coordination and win rate

**Calculation Method:**
1. Left joins with teammates in same round
2. Left joins kills to detect timing coordination
3. Calculates time differences between kills (ABS)
4. Aggregates coordination metrics

**SQL Key Logic:**
```sql
-- Detect coordinated kills
LEFT JOIN Kills teammate_k ON teammate.Id = teammate_k.KillerId
    AND teammate_k.RoundId = r.Id
    AND k.Id IS NOT NULL
    AND ABS(k.GameTime - teammate_k.GameTime) <= 5.0

-- Coordination effectiveness
(CoordinatedKills * 100.0 / TotalKills) * (WinRate) / 100.0
```

---

### 21. Pressure Metrics

**Route:** `GET /api/advanced-analytics/pressure-metrics`

**Purpose:** High-pressure performance, clutch scaling, mental resilience under numerical disadvantage.

**Data Sources:**
- `Players` - Player information
- `PlayerRoundStats` - Performance data
- `Rounds` - Round outcomes with live player counts
- `DemoFiles` - Demo metadata

**Key Metrics:**
- `NumericalSituation` - Classification (1vX_CT, 1vX_T, 2vX_CT, 2vX_T, Neutral)
- `AvgPressureLevel` - Calculated pressure intensity
- `HighPressureSituations` - Count of intense scenarios
- `ClutchAttempts` / `ClutchSuccessRate` - 1vX performance
- `SurvivalRate` - Survival under pressure
- `PressurePerformanceScore` - 0-100 composite score

**Calculation Method:**
1. Uses round live player counts (CTLivePlayers, TLivePlayers)
2. Classifies numerical situations
3. Simplified pressure level based on kills and survival
4. Filters to non-neutral situations
5. Scores based on win rate and K/D under pressure

**SQL Key Logic:**
```sql
-- Numerical situation detection
CASE
    WHEN IsAlive = 1 AND Team = 'CT' AND CTLivePlayers = 1 AND TLivePlayers >= 2 THEN '1vX_CT'
    WHEN IsAlive = 1 AND Team = 'T' AND TLivePlayers = 1 AND CTLivePlayers >= 2 THEN '1vX_T'
    ...
END as NumericalSituation

-- Pressure scoring
CASE
    WHEN WinRate >= 70 AND KDRatio >= 1.5 THEN 95
    WHEN WinRate >= 60 AND KDRatio >= 1.2 THEN 80
    ...
END
```

---

### 22. Database Diagnostic

**Route:** `GET /api/advanced-analytics/database-diagnostic`

**Purpose:** Database health check showing record counts for main tables.

**Data Sources:**
- `DemoFiles`
- `Rounds`
- `Players`
- `Kills`
- `PlayerRoundStats`

**Key Metrics:**
- `RecordCount` - Total records per table
- `MaxId` / `MinId` - ID ranges

**Calculation Method:**
Simple UNION ALL query across main tables.

**SQL Key Logic:**
```sql
SELECT 'TableName', COUNT(*), MAX(Id), MIN(Id) FROM Table
UNION ALL
...
```

**Note:** This endpoint does NOT accept standard query parameters.

---

### 23. Economy Intelligence Dashboard

**Route:** `GET /api/advanced-analytics/economy-intelligence-dashboard`

**Purpose:** Advanced economy analysis including money efficiency, equipment ROI, and economic state correlation.

**Data Sources:**
- `PlayerRoundStats` - Overall stats
- `Players` - Player information
- `Rounds` - Round outcomes
- `DemoFiles` - Demo metadata
- `EconomyEvents` - Money and equipment events

**Key Metrics:**
- `EconomyState` - Money classification (Rich $5000+, Full Buy, Force Buy, Eco, Broke)
- `AvgStartMoney` / `AvgEndMoney` / `AvgMoneySpent` - Money flow
- `AvgItemsEquipped` / `AvgItemsPickedUp` - Equipment stats
- `RifleUsageRate`, `ArmorUsageRate` - Equipment preferences
- `AvgGrenadesPerRound` - Utility usage
- `EconomicEfficiency` - Wins per $1000 invested
- `MoneyManagementScore` - Win rate vs money spent
- `DamagePerDollar`, `EquipmentROI` - Value metrics

**Calculation Method:**
1. **PlayerOverallStats CTE:** Baseline performance
2. **EconomyPerformance CTE:** Detailed equipment events
3. **RoundEconomyStats CTE:** Aggregates per round
4. Joins with overall stats for context
5. Calculates efficiency ratios

**SQL Key Logic:**
```sql
-- Economic efficiency
Wins / AvgStartMoney * 1000 as EconomicEfficiency -- Wins per $1000

-- Equipment value estimation
(RifleCount * 2700 + ArmorCount * 1000 + GrenadeAvg * 300) / RoundCount
```

---

### 24. Advanced Player Performance

**Route:** `GET /api/advanced-analytics/advanced-player-performance`

**Purpose:** HLTV-style ratings, consistency scoring, and multi-kill performance analysis.

**Data Sources:**
- `PlayerRoundStats` - Round-level stats
- `Players` - Player information
- `Rounds` - Round outcomes
- `DemoFiles` - Demo metadata

**Key Metrics:**
- `AvgRating` - Standard rating
- `AvgImpactRating` - Custom HLTV-style calculation
- `QuadKills`, `TripleKills`, `DoubleKills` - Multi-kill counts
- `RatingConsistency` - STDEV of rating
- `WorstRating` / `BestRating` - Performance bounds
- `SurvivalRate` - Survival percentage
- `EfficiencyScore` - ImpactRating * (1 - STDEV/Avg)

**Calculation Method:**
1. Calculates custom impact rating per round
2. Aggregates statistics across rounds
3. Uses STDEV for consistency measurement
4. Efficiency score penalizes inconsistency

**SQL Key Logic:**
```sql
-- Custom HLTV-style impact rating
(Kills * 0.679 + Assists * 0.154 + Damage * 0.0021 +
 CASE WHEN IsAlive = 1 THEN 0.15 ELSE 0 END) as ImpactRating

-- Efficiency with consistency penalty
AvgImpactRating * (1 - (STDEV(Rating) / AVG(Rating)))
```

---

### 25. Player Inventory

**Route:** `GET /api/advanced-analytics/player-inventory`

**Purpose:** Detailed round-by-round equipment, economy, and loadout analysis.

**Data Sources:**
- `EconomyEvents` - Equipment events
- `Players` - Player information
- `DemoFiles` - Demo metadata

**Key Metrics:**
- `RoundStartMoney` / `RoundEndMoney` - Money flow
- `TotalEquipmentValue` - Estimated total value
- `MoneySpentOnEquipment` - Actual purchases
- `UniqueItemsCarried` - Item variety
- `HasPrimary`, `HasArmor` - Equipment flags
- `GrenadeCount` - Utility count
- `WeaponsCarried`, `GrenadesCarried` - Item lists (STRING_AGG)
- `LoadoutCategory` - Classification (Full Buy, Buy Round, Force Buy, Eco, Save)

**Calculation Method:**
1. **PlayerInventory CTE:** Categorizes equipment and estimates values
2. **LatestInventory CTE:** Deduplicates to latest per item
3. **RoundInventorySummary CTE:** Aggregates per round
4. Uses STRING_AGG for item lists
5. Categorizes loadout based on total value

**SQL Key Logic:**
```sql
-- Item value estimation
CASE ItemName
    WHEN 'ak47' THEN 2700
    WHEN 'm4a1_silencer' THEN 3100
    WHEN 'awp' THEN 4750
    ...
END as EstimatedValue

-- Loadout categorization
CASE
    WHEN TotalEquipmentValue >= 6000 THEN 'Full Buy'
    WHEN TotalEquipmentValue >= 3000 THEN 'Buy Round'
    ...
END
```

---

### 26. Master Analytics Dashboard

**Route:** `GET /api/advanced-analytics/master-analytics-dashboard`

**Purpose:** Comprehensive multi-dimensional analysis integrating combat, utility, economy, and intelligence.

**Data Sources:**
- `Players` - Player information
- `DemoFiles` - Demo metadata
- `Matches` - Match data
- `Rounds` - Round context
- `PlayerRoundStats` - Round performance
- `Kills` - Combat events
- `WeaponFires` - Shot accuracy
- `Grenades` - Utility usage
- `FlashEvents` - Flash effectiveness

**Key Metrics:**
- Core: `TotalKills`, `TotalDeaths`, `KDRatio`, `TotalDamage`
- Combat: `Headshots`, `HeadshotPercentage`, `WallbangKills`, `SmokeKills`
- Weapon: `ShotsFired`, `KillsPerShotPercentage`, `AvgShotAccuracy`
- Utility: `GrenadesThrown`, `GrenadeDamageDealt`, `UtilityEfficiency`
- Flash: `TimesFlashed`, `AvgFlashDuration`
- Performance: `SurvivalRate`, `KASTPercentage`, `MVPRounds`, `ClutchRounds`
- `OverallPerformanceScore` - Composite score (Rating*25 + KD*10 + Survival*0.25 + HS*0.25)

**Calculation Method:**
1. Large multi-table left join from Players
2. Aggregates all available metrics
3. Comprehensive scoring formula

**SQL Key Logic:**
```sql
-- Overall performance score
(AvgRating * 25 +
 LEAST(KDRatio * 10, 25) +
 LEAST(SurvivalRate * 0.25, 25) +
 LEAST(HeadshotPercentage * 0.25, 25)) as OverallPerformanceScore
```

---

### 27. Situation Analysis

**Route:** `GET /api/advanced-analytics/situation-analysis`

**Purpose:** Deep dive into performance across game situations with pressure performance scoring.

**Data Sources:**
- `Players` - Player information
- `DemoFiles` - Demo metadata
- `PlayerRoundStats` - Round stats
- `Rounds` - Round context with live players
- `EconomyEvents` - Money state
- `FlashEvents` - Utility state

**Key Metrics:**
- `SituationType` - Classification (1vX_CT, 1vX_T, 2vX_CT, PostPlant_Alive, etc.)
- `EconomyState` - Money classification (Rich, Full_Buy, Force, Eco, Save)
- `UtilityState` - Flash condition (Flashed, Clear)
- `TimesInSituation` - Frequency count
- `WinPercentageInSituation` - Success rate
- `KDRatioInSituation` - Combat effectiveness
- `DifficultyScore` - 0-10 score based on situation
- `PressurePerformanceScore` - Rating with difficulty multipliers

**Calculation Method:**
1. **SituationalPerformance CTE:** Classifies all situations
2. Aggregates performance per situation combination
3. Assigns difficulty scores to situations
4. Applies multipliers to ratings (1vX = 3x, 2vX = 2x, Eco = 1.5x)

**SQL Key Logic:**
```sql
-- Difficulty assignment
CASE
    WHEN SituationType LIKE '1vX_%' THEN 9.0
    WHEN SituationType LIKE '2vX_%' THEN 7.0
    WHEN EconomyState = 'Save' THEN 8.0
    ...
END as DifficultyScore

-- Pressure performance with multipliers
CASE
    WHEN SituationType LIKE '1vX_%' THEN Rating * 3.0
    WHEN SituationType LIKE '2vX_%' THEN Rating * 2.0
    ...
END
```

---

### 28. Economy Intelligence Enhanced

**Route:** `GET /api/advanced-analytics/economy-intelligence-enhanced`

**Purpose:** Advanced economic analysis with ROI, money management, and purchase effectiveness.

**Data Sources:**
- `EconomyEvents` - Money and purchase events
- `Players` - Player information
- `DemoFiles` - Demo metadata
- `Rounds` - Round outcomes
- `PlayerRoundStats` - Performance context

**Key Metrics:**
- `EconomyState` - Money classification (Full_Buy, Force_Buy, Eco_Buy, Save_Round)
- `ItemCategory` - Equipment type (Rifle, Sniper, Pistol, Utility, Armor)
- `PurchaseCount` / `TotalSpent` / `AvgItemCost` - Spending statistics
- `WinRateWithPurchase` - Purchase success correlation
- `KillsPerThousandSpent` - Kill efficiency
- `DamagePerDollarSpent` - Damage efficiency
- `MoneyManagementScore` - Tiered score based on win rate and item category

**Calculation Method:**
1. **EconomyAnalysis CTE:** Joins economy events with performance
2. Categorizes items and money states
3. Calculates ROI metrics (kills/damage per dollar)
4. Scores money management with category-specific thresholds

**SQL Key Logic:**
```sql
-- Kill efficiency
TotalKills * 1.0 / TotalSpent * 1000 as KillsPerThousandSpent

-- Money management scoring
CASE
    WHEN WinRate >= 70 AND ItemCategory = 'Rifle' THEN 95
    WHEN WinRate >= 60 AND EconomyState = 'Force_Buy' THEN 85
    ...
END
```

---

### 29. Movement & Positioning

**Route:** `GET /api/advanced-analytics/movement-positioning`

**Purpose:** Death zone analysis, positioning advantages, and map control patterns.

**Data Sources:**
- `Kills` - Death positions
- `Players` - Victim information
- `Rounds` - Round context
- `DemoFiles` - Map data

**Key Metrics:**
- `MapArea` - Zone classification (A_Site, B_Site, Mid, Tunnels, Connector)
- `PositionType` - Advantage type (Long_Range_Position, Close_Range_Position, Elevation_Advantage)
- `DeathCount` - Deaths in area
- `AvgDeathX/Y/Z` - Average death coordinates
- `AvgKillDistance` - Kill range in area
- `HeadshotDeathRate` - Headshot frequency
- `WeaponsUsed` - Weapon variety
- `PositionDangerScore` - 0-100 composite danger rating

**Calculation Method:**
1. **PositionData CTE:** Maps death positions to areas
2. Classifies position types based on distance and elevation
3. Aggregates death statistics per area
4. Scores danger based on death count and headshot rate

**SQL Key Logic:**
```sql
-- Position type classification
CASE
    WHEN Distance > 1000 THEN 'Long_Range_Position'
    WHEN Distance < 300 THEN 'Close_Range_Position'
    WHEN ABS(KillerZ - VictimZ) > 100 THEN 'Elevation_Advantage'
    ...
END

-- Danger scoring
CASE
    WHEN DeathCount >= 20 AND HeadshotRate >= 60 THEN 95
    WHEN DeathCount >= 15 AND AvgDistance < 500 THEN 85
    ...
END
```

---

### 30. Timing & Tempo

**Route:** `GET /api/advanced-analytics/timing-tempo`

**Purpose:** Round timing patterns, execution speed, and tempo effectiveness analysis.

**Data Sources:**
- `Kills` - Kill timing
- `Players` - Player information
- `Rounds` - Round context
- `DemoFiles` - Map data

**Key Metrics:**
- `RoundPhase` - Time classification (Opening, Mid_Round, Late_Round, Overtime)
- `TempoStyle` - Strategy classification (Aggressive_Rush, Standard_Execute, Slow_Default, Late_Round_Play)
- `AvgKillTiming` - Average kill time
- `AvgTimeBetweenKills` - Kill pace
- `FirstKills` / `LastKills` - Kill sequence position
- `TempoWinRate` - Success rate by tempo
- `TempoMasteryScore` - 0-100 score with tempo-specific bonuses

**Calculation Method:**
1. **TimingData CTE:** Uses LAG() to calculate time between kills
2. ROW_NUMBER() for kill sequence identification
3. Categorizes phases and tempo styles
4. Scores mastery with bonuses for specific tempos (Rush+70% WR = 95)

**SQL Key Logic:**
```sql
-- Time between kills
LAG(GameTime) OVER (PARTITION BY RoundId ORDER BY GameTime) as PrevKillTime
GameTime - PrevKillTime as TimeBetweenKills

-- Tempo classification
CASE
    WHEN GameTime <= 15 THEN 'Aggressive_Rush'
    WHEN GameTime <= 45 THEN 'Standard_Execute'
    ...
END
```

---

### 31. Weapon Mastery

**Route:** `GET /api/advanced-analytics/weapon-mastery`

**Purpose:** Detailed weapon performance with accuracy, versatility, and progression tracking.

**Data Sources:**
- `Kills` - Weapon kills
- `Players` - Player information
- `Rounds` - Round context
- `DemoFiles` - Map data

**Key Metrics:**
- `WeaponClass` - Classification (Assault_Rifle, Sniper_Rifle, Pistol, SMG, Knife)
- `EngagementRange` - Distance category (Long_Range, Medium_Range, Short_Range, Close_Range)
- `KillsWithWeapon` - Usage count
- `AvgKillDistance` / `MinKillDistance` / `MaxKillDistance` - Range statistics
- `HeadshotPercentage` - Precision
- `WinRateWithWeapon` - Weapon effectiveness
- `WeaponMasteryScore` - 0-100 score with weapon-specific thresholds
- `RangeVersatility` - Count of distinct ranges used

**Calculation Method:**
1. Categorizes weapons and ranges
2. Aggregates per player/weapon/range
3. Scores mastery with weapon-specific requirements

**SQL Key Logic:**
```sql
-- Weapon mastery scoring
CASE
    WHEN WeaponClass = 'Sniper_Rifle' AND HeadshotPercentage >= 80 THEN 100
    WHEN WeaponClass = 'Assault_Rifle' AND HeadshotPercentage >= 60 AND Kills >= 20 THEN 95
    WHEN WeaponClass = 'Pistol' AND HeadshotPercentage >= 70 THEN 90
    ...
END
```

---

### 32. Match Flow

**Route:** `GET /api/advanced-analytics/match-flow`

**Purpose:** Momentum patterns, comeback potential, and performance under pressure analysis.

**Data Sources:**
- `Rounds` - Round sequences
- `DemoFiles` - Demo context
- `PlayerRoundStats` - Performance data
- `Players` - Player information

**Key Metrics:**
- `MomentumState` - Classification (Hot_Streak, Building_Momentum, Cold_Streak, Losing_Momentum, Comeback_Round)
- `GameState` - Score context (Close_Game, Leading, Trailing, Balanced)
- `WinRateInState` - Success rate per state
- `AvgKillsInState` / `AvgDeathsInState` - Performance under conditions
- `ClutchRounds` - High-impact rounds
- `CloseGameWins` - Performance in tight situations
- `MomentumMasteryScore` - 0-100 score with momentum-specific bonuses

**Calculation Method:**
1. **MatchFlowData CTE:** Uses LAG() for previous 2 round outcomes
2. Calculates running team score with SUM() OVER
3. **MomentumAnalysis CTE:** Classifies momentum and game states
4. Scores mastery with bonuses (Comeback+70% = 100, Hot Streak+2.0KD = 95)

**SQL Key Logic:**
```sql
-- Momentum classification
CASE
    WHEN RoundWon = 1 AND PrevRoundWon = 1 AND TwoRoundsAgo = 1 THEN 'Hot_Streak'
    WHEN RoundWon = 1 AND PrevRoundWon = 0 THEN 'Comeback_Round'
    ...
END

-- Score context
CASE
    WHEN ABS(TeamScore - OpponentScore) <= 1 THEN 'Close_Game'
    ...
END
```

---

### 33. Performance Trends

**Route:** `GET /api/advanced-analytics/performance-trends`

**Purpose:** Player improvement over time, consistency metrics, and learning curve analysis.

**Data Sources:**
- `PlayerRoundStats` - Round-level performance
- `Players` - Player information
- `Rounds` - Round context
- `DemoFiles` - Temporal data (ParsedAt)

**Key Metrics:**
- `TimeQuintile` - Chronological period (1-5, splits career into fifths)
- `AvgKills` / `AvgDeaths` / `AvgDamage` / `AvgKDRatio` - Performance over time
- `WinRate` - Success rate trend
- `KillsStdDev` / `DamageStdDev` - Consistency metrics
- `MaxMapExperience` - Map-specific experience level
- `TrendScore` - 0-100 score (higher for recent performance)

**Calculation Method:**
1. Uses ROW_NUMBER() for chronological ordering
2. NTILE(5) splits career into 5 periods
3. Separate map experience tracking
4. Higher scores for later quintiles (shows improvement)

**SQL Key Logic:**
```sql
-- Chronological splitting
ROW_NUMBER() OVER (PARTITION BY PlayerId ORDER BY ParsedAt, RoundNumber) as ChronologicalOrder
NTILE(5) OVER (PARTITION BY PlayerId ORDER BY ParsedAt, RoundNumber) as TimeQuintile

-- Trend scoring favors recent performance
CASE
    WHEN TimeQuintile = 5 THEN [higher scores based on metrics]
    ...
END
```

---

### 34. Team Dynamics

**Route:** `GET /api/advanced-analytics/team-dynamics`

**Purpose:** Player partnerships, trade kills, support plays, and team chemistry analysis.

**Data Sources:**
- `PlayerRoundStats` (prs1, prs2) - Player and teammate stats
- `Players` (p1, p2) - Player and teammate info
- `Rounds` - Round context
- `DemoFiles` - Demo metadata
- `Kills` (k1, k2) - Kill timing for coordination

**Key Metrics:**
- `RoundsPlayedTogether` - Partnership frequency
- `TeamWinRate` - Combined success rate
- `AvgKillsWithTeammate` / `AvgDamageWithTeammate` - Player performance
- `TeammateAvgKills` / `TeammateAvgDamage` - Teammate performance
- `TradeKillsExecuted` / `TradeKillRate` - Revenge coordination
- `SupportPlays` - Assist synergy
- `AvgKillCoordination` - Kill timing correlation
- `TeamChemistryScore` - 0-100 composite score

**Calculation Method:**
1. Self-joins PlayerRoundStats to find teammates in same rounds
2. Left joins Kills for both players to detect timing
3. Detects trade kills (within 3 seconds)
4. Identifies support plays (assists + teammate positive KD)
5. Scores chemistry based on win rate, trades, and coordination

**SQL Key Logic:**
```sql
-- Teammate identification
INNER JOIN PlayerRoundStats prs2 ON r.Id = prs2.RoundId AND prs2.PlayerId != prs1.PlayerId
INNER JOIN Players p2 ON prs2.PlayerId = p2.Id AND p2.Team = p1.Team

-- Trade kill detection
CASE
    WHEN ABS(k1.GameTime - k2.GameTime) <= 3.0 AND k1/k2 NOT NULL THEN 1
    ELSE 0
END as TradeKill

-- Chemistry scoring
CASE
    WHEN WinRate >= 70 AND TradeKills >= 5 THEN 100
    WHEN WinRate >= 60 AND SupportPlays >= 10 THEN 90
    ...
END
```

---

## Response Format

All endpoints return JSON in this format:

```json
{
  "Title": "Endpoint Title",
  "Description": "Description of analysis",
  "Data": [
    {
      "Column1": "Value1",
      "Column2": 123,
      ...
    }
  ],
  "TotalRecords": 42
}
```

### CSV Export

Add `?format=csv` to any endpoint to receive CSV output:
- Automatic filename with timestamp
- Content-Disposition header for download
- Proper CSV escaping for commas and quotes

---

## Database Schema Reference

### Core Tables Used

| Table | Purpose |
|-------|---------|
| `DemoFiles` | Demo metadata (MapName, ParsedAt, FileName) |
| `Rounds` | Round information (RoundNumber, WinnerTeam, BombPlanted, LivePlayers) |
| `Players` | Player information (PlayerName, Team, DemoFileId) |
| `Kills` | Kill events (Killer, Victim, Weapon, Position, Distance, IsHeadshot, IsFirstKill) |
| `PlayerRoundStats` | Round-level stats (Kills, Deaths, Assists, Damage, EquipmentValue, IsAlive) |
| `Grenades` | Grenade events (GrenadeType, ThrowPosition, DetonatePosition, ThrowTick) |
| `FlashEvents` | Flash effectiveness (FlasherPlayerId, FlashedPlayerId, FlashDuration) |
| `WeaponFires` | Shot events (PlayerId, Weapon, GameTime, Accuracy) |
| `Equipment` | Equipment changes (PlayerId, ItemName, Action, Tick) |
| `EconomyEvents` | Economy events (PlayerId, EventType, ItemName, MoneyBefore, MoneyAfter) |
| `EnhancedPlayerPositions` | Position tracking (PlayerId, PositionX/Y/Z, VelocityX/Y, Tick) |
| `Matches` | Match metadata |

---

## Common SQL Patterns

### CTEs (Common Table Expressions)
Most endpoints use multi-stage CTEs for complex calculations:
1. **Data Collection CTE:** Gathers base data with joins
2. **Calculation CTE:** Performs aggregations and window functions
3. **Scoring CTE:** Applies business logic for ratings
4. **Final SELECT:** Aggregates and filters results

### Window Functions
Heavily used for:
- `ROW_NUMBER()` - Sequencing and ranking
- `LAG()` / `LEAD()` - Temporal comparisons
- `SUM() OVER ()` - Running totals
- `NTILE()` - Percentile bucketing

### Spatial Analysis
Position-based queries use:
- Pythagorean distance: `SQRT(POWER(X2-X1, 2) + POWER(Y2-Y1, 2))`
- Map area classification via CASE statements on coordinates
- STDEV for position spread/clustering

### Scoring Methodology
Most endpoints use tiered CASE statements for scoring:
```sql
CASE
    WHEN [excellent criteria] THEN 95-100
    WHEN [very good criteria] THEN 80-90
    WHEN [good criteria] THEN 65-75
    WHEN [acceptable criteria] THEN 50-60
    ELSE 30-45
END as Score
```

---

## Performance Considerations

1. **Indexing:** Queries assume indexes on foreign keys (PlayerId, RoundId, DemoFileId)
2. **Timeouts:** Command timeout set to 120 seconds
3. **Result Limits:** Most queries use `HAVING COUNT(*) >= N` to filter insignificant data
4. **Parameter Safety:** All queries use parameterized inputs to prevent SQL injection

---

## Error Handling

All endpoints return:
- **200 OK:** Successful query with data
- **500 Internal Server Error:** Query failure with error message in JSON

Error response format:
```json
{
  "Error": "Error message",
  "Details": "Detailed exception message"
}
```

---

## Development Notes

- **Controller Location:** `CS2DemoParserWeb/Controllers/AdvancedAnalyticsController.cs`
- **Connection String:** Configured via `appsettings.json` or `CONNECTION_STRING` environment variable
- **Logging:** Uses ILogger for error tracking
- **SQL Server:** Queries written for Microsoft SQL Server with T-SQL syntax

---

**Last Updated:** 2025-10-01
**Total Endpoints:** 34
**Lines of Code:** ~4,890
