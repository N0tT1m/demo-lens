# 🎯 Demo Lens - CS2 Demo Analysis Tool

**Demo Lens** is a powerful web application that analyzes your Counter-Strike 2 demo files and provides detailed insights into your gameplay. Upload your `.dem` files and get comprehensive statistics, heatmaps, and reports to improve your performance.

![Demo Lens Banner](https://via.placeholder.com/800x200/1e293b/ffffff?text=Demo+Lens+-+CS2+Demo+Analysis)

## 📥 Download & Installation

### Quick Start (Docker - Recommended)
1. **Install Docker**: [Download Docker Desktop](https://www.docker.com/get-started/)
2. **Download Demo Lens**: 
   ```bash
   git clone https://github.com/yourusername/demo-lens.git
   cd demo-lens
   
   # Clone required demofile-net dependency
   git clone https://github.com/saul/demofile-net.git
   ```
3. **Run Application**:
   ```bash
   docker-compose up --build
   ```
4. **Open Browser**: Go to http://localhost:8080

### Direct Download (No Docker)
1. **Download Latest Release**: [GitHub Releases](https://github.com/yourusername/demo-lens/releases)
2. **Extract Files** to desired location
3. **Clone demofile-net dependency**:
   ```bash
   cd demo-lens-folder
   git clone https://github.com/saul/demofile-net.git
   ```
4. **Install .NET 9**: [Download .NET Runtime](https://dotnet.microsoft.com/download)
5. **Run Application**: Double-click `CS2DemoParserWeb.exe` (Windows) or run `dotnet CS2DemoParserWeb.dll`
6. **Open Browser**: Use the URL shown in console (usually http://localhost:5000)

## ✨ Features

### 📊 **Comprehensive Analysis**
- **Player Statistics** - Kills, deaths, K/D ratio, headshot percentage, damage dealt
- **Round Analysis** - Round-by-round breakdowns, win conditions, economy tracking
- **Weapon Statistics** - Performance with different weapons, accuracy metrics
- **Team Performance** - CT vs T side analysis, economy management

### 🗺️ **Interactive Heatmaps**
- **Death Locations** - See where players die most frequently
- **Kill Positions** - Analyze common frag spots
- **Player Movement** - Track positioning throughout rounds
- **Weapon Fire** - Visualize shooting patterns
- **Utility Usage** - Grenade throws and effectiveness

### 📈 **Advanced Reporting**
- **Export Data** - Download statistics as CSV or JSON
- **Visual Query Builder** - Create custom reports without SQL knowledge
- **Advanced SQL Mode** - Power users can write custom queries
- **Downloadable Heatmaps** - Save heatmaps with legends as images

### 🔍 **Smart Features**
- **Auto-fill Values** - Smart suggestions based on your actual data
- **Multiple Demo Sources** - Supports Matchmaking, Faceit, ESEA demos
- **Real-time Analysis** - Instant results as demos are processed
- **Responsive Design** - Works on desktop, tablet, and mobile

## 🚀 Local Development & Configuration

### Development Environment
```bash
# Clone repository
git clone https://github.com/yourusername/demo-lens.git
cd demo-lens

# Clone required demofile-net dependency
git clone https://github.com/saul/demofile-net.git

# Start local development (includes SQL Server)
docker-compose up -d

# Access services
# Web App: http://localhost:8080
# Database: localhost:1433 (sa/DemoParser123!)
```

### Configuration Files
- **NEVER commit** `appsettings.Development.json` or `appsettings.Production.json`
- Use `appsettings.Example.json` as template for local settings
- All secrets are managed through Docker environment variables

## 📱 How to Use

### 1. Upload Your Demo
1. Click **"Upload Demo File"**
2. Select your `.dem` file from CS2
3. Choose the **demo source** (Matchmaking, Faceit, ESEA, etc.)
4. Click **"Upload Demo"**

### 2. View Statistics
- Browse **player stats**, **round summaries**, and **weapon analysis**
- Check the **database statistics** to see your collection grow

### 3. Explore Heatmaps
1. Select a **map** from the dropdown
2. Choose a **demo file** (or view all)
3. Pick **heatmap type** (deaths, kills, movement, etc.)
4. Click **"Generate Heatmap"**
5. Download heatmaps with the **download button**

### 4. Generate Reports
1. Go to **"Export Reports"** section
2. Choose report type (kills, economy, player stats, etc.)
3. Set optional filters (date range, player, map)
4. Download as **CSV** or **JSON**

### 5. Query Builder
1. Use the **"Demo Query Builder"** for custom analysis
2. **Visual Mode**: Point-and-click interface for non-technical users
3. **Advanced Mode**: Write SQL queries for power users
4. Auto-suggestions based on your actual data

## 🎮 Supported Demo Sources

| Source | Description | Notes |
|--------|-------------|-------|
| **Matchmaking** | Official Valve servers | Standard format |
| **Faceit** | Faceit platform demos | Includes warmup + knife round |
| **ESEA** | ESEA league demos | Includes warmup + knife round |
| **Other** | Community servers | Generic parsing |

## 📊 Available Reports

### Player Statistics
- Kills, deaths, assists, K/D ratio
- Headshot percentage and accuracy
- Damage dealt and taken
- Economy efficiency

### Match Analysis
- Round-by-round breakdowns
- Win conditions and patterns
- Team performance comparison
- Map-specific statistics

### Weapon Analysis
- Performance per weapon
- Headshot rates by gun
- Most effective weapons
- Usage patterns

### Economy Tracking
- Money management
- Buy round efficiency
- Force buy analysis
- Equipment value tracking

## 🔧 Troubleshooting

### Demo Upload Issues
- **File too large**: Maximum 1GB per file
- **Invalid format**: Ensure file is a CS2 `.dem` file
- **Parsing errors**: Try different demo source setting

### Performance Issues
- **Slow loading**: Large demos take time to process
- **Memory usage**: Close other applications during processing
- **Storage space**: Ensure adequate disk space for database

### Browser Issues
- **Blank page**: Try refreshing or different browser
- **Features not working**: Enable JavaScript
- **Slow response**: Check console for errors

### Docker Issues
```bash
# View logs
docker-compose logs -f

# Restart services
docker-compose restart

# Clean rebuild
docker-compose down && docker-compose up --build

# Remove all data
docker-compose down -v
```

### Database Issues
```bash
# Access local SQL Server
docker exec -it sqlserver sqlcmd -S localhost -U sa -P DemoParser123!

# Check database connection
docker-compose logs web | grep -i sql

# Reset database
docker-compose down -v && docker-compose up -d
```

## 💡 Tips for Best Results

### Demo Collection
- **Regular uploads**: Build a comprehensive database
- **Consistent naming**: Use descriptive filenames
- **Various maps**: Analyze performance across different maps
- **Different periods**: Track improvement over time

### Analysis Workflow
1. **Upload recent demos** regularly
2. **Review heatmaps** to identify patterns
3. **Export statistics** for deeper analysis
4. **Compare performance** across different time periods
5. **Focus on weak areas** identified in reports

### Query Builder Tips
- **Start simple**: Use visual mode for basic queries
- **Explore data**: Check "Show Available Tables" to understand structure
- **Use filters**: Narrow down results with date ranges and player names
- **Export results**: Save interesting findings as CSV/JSON


## 🔐 Security & Privacy

### Development Security
- **Never commit** real passwords, API keys, or connection strings
- Copy `appsettings.Example.json` to `appsettings.Development.json` for local config
- All secrets are managed through Docker environment variables

### Data Privacy
This application processes CS2 demo files which may contain:
- Player names and Steam IDs
- Game statistics and performance data
- Voice chat transcripts (if enabled)

### Local Storage
- **SQL Server database**: Data stored in Docker volume
- **Upload files**: Demo files stored in `./uploads/` directory
- **No cloud uploads**: All processing happens locally
- **Export anytime**: Full control over your data

## 🆘 Support

### Common Questions
- **"Heatmaps not showing"**: Ensure demo has position data
- **"Economy data empty"**: Some demos may lack economy events
- **"Query returns no results"**: Check table names and filters

### Getting Help
- Check the **SQL Cheat Sheet** for query examples
- Use **"Show Available Tables"** to explore database structure
- Review console logs for detailed error messages

## 📄 License

Demo Lens is open source software. See LICENSE file for details.

---

**Demo Lens** - Analyze, Improve, Dominate 🏆

*Transform your CS2 demos into actionable insights and take your gameplay to the next level.*