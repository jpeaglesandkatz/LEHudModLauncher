# --- Universal PowerShell Release Publisher ---
# Version: 3.0 (Elegant & Stateful)
# Purpose: A universal script to commit, tag, and create a new release on GitHub for ANY project.
# - Uses a single 'publish.config.json' file to remember project name and version.
# - To reset, simply delete the 'publish.config.json' file.

[CmdletBinding()]
param (
    [Switch]$Reset
)

# --- Configuration ---
$RepoRoot = $PSScriptRoot
$ConfigFilePath = Join-Path $RepoRoot "publish.config.json"
$InitialVersion = "1.0.0"

# --- Script Start ---
Clear-Host
Write-Host "====================================================" -ForegroundColor Green
Write-Host "     Universal Project Release Publisher"
Write-Host "====================================================" -ForegroundColor Green
Write-Host "Workspace: $RepoRoot" -ForegroundColor Cyan
Write-Host ""

if ($Reset) {
    if (Test-Path $ConfigFilePath) { Remove-Item $ConfigFilePath -Force; Write-Host "   Removed .publish-config" }
    Write-Host "Reset complete." -ForegroundColor Green
    return
}

if (-not (Test-Path (Join-Path $RepoRoot ".git"))) { Write-Host "[FATAL ERROR] This script must be run from inside a cloned Git repository." -ForegroundColor Red; return }
$gitExists = Get-Command git -ErrorAction SilentlyContinue; if (-not $gitExists) { Write-Host "[FATAL ERROR] Git not found." -ForegroundColor Red; return }
$ghExists = Get-Command gh -ErrorAction SilentlyContinue; if (-not $ghExists) { Write-Host "[FATAL ERROR] GitHub CLI (gh) not found." -ForegroundColor Red; return }

$ProjectName = ""; $suggestedVersion = $InitialVersion
if (Test-Path $ConfigFilePath) {
    try {
        $config = Get-Content $ConfigFilePath -Raw | ConvertFrom-Json
        $ProjectName = $config.ProjectName
        $lastVersion = $config.Version
        Write-Host "-> Found 'publish.config.json'. Loading existing configuration..."
        Write-Host "   Project Name: '$ProjectName'" -ForegroundColor Cyan
        Write-Host "   Last Saved Version: $lastVersion"
        $versionParts = $lastVersion.Split('.'); if ($versionParts.Count -ge 3) { $newPatchNumber = ([int]$versionParts[-1]) + 1; $versionParts[-1] = $newPatchNumber.ToString(); $suggestedVersion = $versionParts -join '.' }
    } catch { Write-Host "[WARNING] 'publish.config.json' is corrupt. Please fix or delete it." -ForegroundColor Yellow; return }
} else {
    Write-Host "-> No 'publish.config.json' found. This must be the first run or a reset." -ForegroundColor Yellow
    $ProjectName = Read-Host "   Please enter a name for this project"
    if (-not $ProjectName) { Write-Host "[ERROR] Project name cannot be empty." -ForegroundColor Red; return }
}

Write-Host "   Suggested next version: $suggestedVersion" -ForegroundColor Yellow
$Version = Read-Host "-> Enter the version number for this release (Press Enter for '$suggestedVersion')"
if (-not $Version) { $Version = $suggestedVersion }

$commitMessage = Read-Host "-> Enter a short commit message (Press Enter for 'Release v$Version')"; if (-not $commitMessage) { $commitMessage = "Release v${Version}" }
$releaseNotes = Read-Host "-> Enter release notes (a short description of what's new)"; if (-not $releaseNotes) { $releaseNotes = "No release notes provided." }

$ReleaseZipPath = Join-Path $RepoRoot "_releases"; if (-not (Test-Path $ReleaseZipPath)) { New-Item $ReleaseZipPath -ItemType Directory -Force | Out-Null }
$ZipFileName = "${ProjectName}_v${Version}.zip"; $ZipFullPath = Join-Path $ReleaseZipPath $ZipFileName
Write-Host "-> Creating ZIP archive..." -ForegroundColor Yellow
$ignoreList = @( ".git*", "_releases", "_build_backups", "_temp_icons", "*.ps1", "*.json" )
Compress-Archive -Path (Get-ChildItem -Path $RepoRoot -Exclude $ignoreList) -DestinationPath $ZipFullPath -Force
Write-Host "   Release archive created."

$currentBranch = git branch --show-current; if (-not $currentBranch) { Write-Host "[FATAL ERROR] Could not determine current Git branch." -ForegroundColor Red; return }
Write-Host "-> Committing and pushing to '$currentBranch'..."
git add .; git commit -m $commitMessage; $tagName = "v$Version"; git tag $tagName; git push origin $currentBranch; git push origin $tagName
Write-Host "   Code and new tag pushed successfully."

Write-Host "-> Creating GitHub Release..."
gh release create $tagName "$ZipFullPath" --title $commitMessage --notes $releaseNotes --latest
Write-Host "   GitHub release created and ZIP uploaded."

$newConfig = @{ ProjectName = $ProjectName; Version = $Version }; $newConfig | ConvertTo-Json -Depth 3 | Set-Content $ConfigFilePath -Encoding UTF8
Write-Host "-> Updated 'publish.config.json' to version $Version." -ForegroundColor Green

Write-Host ""; Write-Host "====================================================" -ForegroundColor Green; Write-Host "     RELEASE COMPLETE"; Write-Host "====================================================" -ForegroundColor Green; Write-Host ""
$repoUrl = git remote get-url origin
if ($repoUrl -and $repoUrl.StartsWith("https:")) { $releaseUrl = $repoUrl.Replace(".git", "/releases/tag/$tagName"); Write-Host "You can view the new release at: $releaseUrl" -ForegroundColor Yellow }