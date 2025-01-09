# Find all project files...
$extensions = @("*.csproj", "*.wapproj", "*.sqlproj")
$projectFiles = Get-ChildItem -Include $extensions -Recurse 

# Get project directories
$projectDirectories = $projectFiles | ForEach-Object { $_.DirectoryName } | Get-Unique

# Remove existing bin-directories
$projectDirectories | ForEach-Object { if ( Test-Path -Path "$($_)\bin" -PathType Container) { Remove-Item "$($_)\bin" -Force -Recurse } }

# Remove existing obj-directories
$projectDirectories | ForEach-Object { if ( Test-Path -Path "$($_)\obj" -PathType Container) { Remove-Item "$($_)\obj" -Force -Recurse } }

pause
