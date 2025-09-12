# PowerShell script to fix the third wave of UWP/WinUI compatibility issues
# These are the remaining 7 errors after the second round of fixes

Write-Host "Starting third wave of XAML compatibility fixes..." -ForegroundColor Green

$xamlPath = "c:\projects\Smartitecture\Smartitecture\Application\UI"
$themesPath = "c:\projects\Smartitecture\Smartitecture\Themes"

$xamlFiles = Get-ChildItem -Path $xamlPath -Filter "*.xaml" -Recurse
$themeFiles = Get-ChildItem -Path $themesPath -Filter "*.xaml" -Recurse
$allFiles = $xamlFiles + $themeFiles

Write-Host "Processing $($allFiles.Count) XAML files for third wave fixes..." -ForegroundColor Yellow

foreach ($file in $allFiles) {
    Write-Host "Processing: $($file.Name)" -ForegroundColor Cyan
    
    $content = Get-Content $file.FullName -Raw
    $originalContent = $content
    
    # Fix StepFrequency property (UWP/WinUI) - remove it from Slider
    $content = $content -replace '\s+StepFrequency="[^"]*"', ''
    
    # Fix duplicate Text attributes (caused by previous script)
    $content = $content -replace '(\s+Text="[^"]*")\s+Text="[^"]*"', '$1'
    
    # Fix IsActive property on ProgressBar (UWP/WinUI) - remove it
    $content = $content -replace '\s+IsActive="[^"]*"', ''
    
    # Fix PivotItem to TabItem (complete the conversion)
    $content = $content -replace '<PivotItem([^>]*)>', '<TabItem$1>'
    $content = $content -replace '</PivotItem>', '</TabItem>'
    
    # Fix SelectionChanged on StackPanel (move to individual RadioButtons or remove)
    $content = $content -replace '(<StackPanel[^>]*)\s+SelectionChanged="[^"]*"([^>]*>)', '$1$2'
    
    # Fix ContentTransitions property (UWP/WinUI) - remove it
    $content = $content -replace '\s+ContentTransitions="[^"]*"', ''
    
    # Additional cleanup for any remaining UWP/WinUI properties
    $content = $content -replace '\s+Transitions="[^"]*"', ''
    $content = $content -replace '\s+DefaultStyleKey="[^"]*"', ''
    
    if ($content -ne $originalContent) {
        Set-Content -Path $file.FullName -Value $content -Encoding UTF8
        Write-Host "  ✓ Fixed third wave compatibility issues" -ForegroundColor Green
    } else {
        Write-Host "  - No changes needed" -ForegroundColor Gray
    }
}

Write-Host "`nThird wave XAML compatibility fixes completed!" -ForegroundColor Green
Write-Host "Run 'dotnet clean && dotnet build' to test the fixes." -ForegroundColor Yellow
