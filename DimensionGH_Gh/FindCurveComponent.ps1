# Script to find CurveComponents.gha file

Write-Host "Searching for CurveComponents.gha files..." -ForegroundColor Yellow
Write-Host ""

$searchPaths = @(
    "C:\Program Files\Rhino 8\Plug-ins\Grasshopper\Components",
    "$env:APPDATA\Grasshopper\Libraries",
    "$env:APPDATA\Grasshopper\Components",
    "C:\Program Files\Rhino 8\Plug-ins\Grasshopper"
)

$foundFiles = @()

foreach ($path in $searchPaths) {
    if (Test-Path $path) {
        Write-Host "Checking: $path" -ForegroundColor Cyan
        try {
            $files = Get-ChildItem -Path $path -Recurse -Filter "*.gha" -ErrorAction SilentlyContinue | Where-Object { $_.Name -match "curve" -or $_.Name -match "Curve" }
            
            if ($files) {
                foreach ($file in $files) {
                    Write-Host "  FOUND: $($file.FullName)" -ForegroundColor Green
                    $foundFiles += $file
                }
            } else {
                Write-Host "  No files found" -ForegroundColor Gray
            }
        } catch {
            Write-Host "  Access error: $_" -ForegroundColor Red
        }
    } else {
        Write-Host "Path does not exist: $path" -ForegroundColor Gray
    }
    Write-Host ""
}

if ($foundFiles.Count -eq 0) {
    Write-Host "CurveComponents.gha files NOT FOUND in standard locations." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Try to find it manually:" -ForegroundColor Yellow
    Write-Host "1. Open Grasshopper"
    Write-Host "2. File -> Special Folders -> Components Folder"
    Write-Host "3. Look for files with 'Curve' in the name"
    Write-Host ""
    Write-Host "Or check Grasshopper logs for errors:" -ForegroundColor Yellow
    Write-Host "  $env:APPDATA\Grasshopper\Logs" -ForegroundColor Cyan
} else {
    Write-Host "`nFOUND files: $($foundFiles.Count)" -ForegroundColor Green
    Write-Host ""
    Write-Host "To delete file(s), run:" -ForegroundColor Yellow
    foreach ($file in $foundFiles) {
        Write-Host "  Remove-Item `"$($file.FullName)`"" -ForegroundColor White
    }
    Write-Host ""
    Write-Host "Or delete manually through Windows Explorer" -ForegroundColor Yellow
}

Write-Host "`nPress any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
