# PowerShell script to fix the 5th wave of UWP/WinUI compatibility issues
# These are the remaining 2 errors after manual fixes

Write-Host "Starting 5th wave of XAML compatibility fixes..." -ForegroundColor Green

$xamlPath = "c:\projects\Smartitecture\Smartitecture\Application\UI"
$themesPath = "c:\projects\Smartitecture\Smartitecture\Themes"

$xamlFiles = Get-ChildItem -Path $xamlPath -Filter "*.xaml" -Recurse
$themeFiles = Get-ChildItem -Path $themesPath -Filter "*.xaml" -Recurse
$allFiles = $xamlFiles + $themeFiles

Write-Host "Processing $($allFiles.Count) XAML files for 5th wave fixes..." -ForegroundColor Yellow

foreach ($file in $allFiles) {
    Write-Host "Processing: $($file.Name)" -ForegroundColor Cyan
    
    $content = Get-Content $file.FullName -Raw
    $originalContent = $content
    
    # Fix remaining OnContent/OffContent properties on CheckBox (UWP/WinUI)
    $content = $content -replace '(<CheckBox[^>]*)\s+OnContent="[^"]*"([^>]*)', '$1$2'
    $content = $content -replace '(<CheckBox[^>]*)\s+OffContent="[^"]*"([^>]*)', '$1$2'
    
    # Fix remaining Toggled events to WPF Checked/Unchecked
    $content = $content -replace '(<CheckBox[^>]*)\s+Toggled="([^"]*)"([^>]*)', '$1 Checked="$2" Unchecked="$2"$3'
    
    # Fix GridView.ItemTemplate to ListBox.ItemTemplate (WPF syntax)
    $content = $content -replace '<GridView\.ItemTemplate>', '<ListBox.ItemTemplate>'
    $content = $content -replace '</GridView\.ItemTemplate>', '</ListBox.ItemTemplate>'
    
    # Fix any remaining GridView references to ListBox
    $content = $content -replace '<GridView([^>]*)>', '<ListBox$1>'
    $content = $content -replace '</GridView>', '</ListBox>'
    
    # Fix any remaining UWP/WinUI properties that might be hiding
    $content = $content -replace '\s+IsItemClickEnabled="[^"]*"', ''
    $content = $content -replace '\s+ItemClick="[^"]*"', ''
    $content = $content -replace '\s+ShowsScrollingPlaceholders="[^"]*"', ''
    
    if ($content -ne $originalContent) {
        Set-Content -Path $file.FullName -Value $content -Encoding UTF8
        Write-Host "  ✓ Fixed 5th wave compatibility issues" -ForegroundColor Green
    } else {
        Write-Host "  - No changes needed" -ForegroundColor Gray
    }
}

Write-Host "`n🎉 5th wave XAML compatibility fixes completed! 🎉" -ForegroundColor Green
Write-Host "Run 'dotnet clean && dotnet build' to test for SUCCESS!" -ForegroundColor Yellow
