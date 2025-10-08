# Jenkins Configuration for FoodBook .NET 9 Project

## 🚀 Jenkins Setup Instructions

### 1. Access Jenkins
- URL: http://localhost:8080
- Default port: 8080

### 2. Initial Setup
1. Get initial admin password from: `D:\Jenkins\secrets\initialAdminPassword`
2. Install suggested plugins
3. Create admin user

### 3. Required Plugins
Install these plugins for .NET development:
- **Git Plugin** (for Git integration)
- **Pipeline Plugin** (for Jenkinsfile support)
- **.NET Plugin** (for .NET build tools)
- **MSBuild Plugin** (for .NET builds)
- **NUnit Plugin** (for test results)
- **Artifact Archiver Plugin** (for build artifacts)

### 4. Global Tool Configuration
Configure these tools in **Manage Jenkins** → **Global Tool Configuration**:

#### Git Configuration
- **Name**: Default
- **Path to Git executable**: `git`

#### .NET SDK Configuration
- **Name**: dotnet-9.0
- **Installation directory**: `C:\Program Files\dotnet` (or your .NET installation path)

### 5. Create New Pipeline Job
1. **New Item** → **Pipeline**
2. **Name**: `FoodBook-CI-CD`
3. **Pipeline script from SCM**:
   - **SCM**: Git
   - **Repository URL**: `https://github.com/TomOutfit/FoodBook_PRN212_Group4_Fa25.git`
   - **Branch**: `*/main`
   - **Script Path**: `Jenkinsfile`

### 6. Build Triggers
Configure automatic builds:
- **Poll SCM**: `H/5 * * * *` (every 5 minutes)
- **GitHub hook trigger** (if using GitHub webhooks)

### 7. Environment Variables
Set these in job configuration:
- `DOTNET_VERSION`: `9.0`
- `BUILD_CONFIGURATION`: `Release`
- `NUGET_PACKAGES`: `~/.nuget/packages`

## 🔧 Troubleshooting

### Jenkins Won't Start
```bash
# Check if Jenkins is running
netstat -an | findstr :8080

# Start Jenkins manually
cd D:\
java -jar jenkins.war --httpPort=8080
```

### .NET Not Found
```bash
# Check .NET installation
dotnet --version

# Install .NET 9 SDK if needed
# Download from: https://dotnet.microsoft.com/download/dotnet/9.0
```

### Git Integration Issues
```bash
# Check Git installation
git --version

# Configure Git if needed
git config --global user.name "Your Name"
git config --global user.email "your.email@example.com"
```

## 📋 Pipeline Features

### Automatic Project Discovery
- Finds all `.sln` files
- Finds all `.csproj` files
- Identifies test projects

### Build Process
1. **Checkout**: Gets latest code from Git
2. **Discover**: Automatically finds .NET projects
3. **Setup**: Configures .NET environment
4. **Restore**: Downloads NuGet packages
5. **Build**: Compiles .NET projects
6. **Test**: Runs unit tests
7. **Publish**: Creates deployment packages
8. **Archive**: Stores build artifacts

### Error Handling
- Pipeline continues even if individual steps fail
- Detailed logging for troubleshooting
- Artifact archiving for successful builds

## 🎯 Benefits

1. **Automated CI/CD**: No manual intervention needed
2. **Flexible**: Works with any .NET project structure
3. **Robust**: Handles missing projects gracefully
4. **Comprehensive**: Full build, test, and deploy pipeline
5. **Integrated**: Works with existing GitHub repository

## 📞 Support

For issues with this Jenkins setup:
1. Check Jenkins logs: `D:\Jenkins\logs\`
2. Verify .NET installation
3. Ensure Git is properly configured
4. Check network connectivity to GitHub
