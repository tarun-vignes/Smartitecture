# PowerShell script to fix the second wave of UWP/WinUI compatibility issues
# These errors were revealed after the first round of fixes

Write-Host "Starting second wave of XAML compatibility fixes..." -ForegroundColor Green

$xamlPath = "c:\projects\Smartitecture\Smartitecture\Application\UI"
$themesPath = "c:\projects\Smartitecture\Smartitecture\Themes"

$xamlFiles = Get-ChildItem -Path $xamlPath -Filter "*.xaml" -Recurse
$themeFiles = Get-ChildItem -Path $themesPath -Filter "*.xaml" -Recurse
$allFiles = $xamlFiles + $themeFiles

Write-Host "Processing $($allFiles.Count) XAML files for second wave fixes..." -ForegroundColor Yellow

foreach ($file in $allFiles) {
    Write-Host "Processing: $($file.Name)" -ForegroundColor Cyan
    
    $content = Get-Content $file.FullName -Raw
    $originalContent = $content
    
    # Fix BorderBrush on StackPanel (doesn't exist in WPF StackPanel)
    $content = $content -replace '(<StackPanel[^>]*)\s+BorderBrush="[^"]*"([^>]*>)', '$1$2'
    
    # Fix Padding on elements that don't support it (replace with Margin)
    $content = $content -replace '(<StackPanel[^>]*)\s+Padding="([^"]*)"([^>]*>)', '$1 Margin="$2"$3'
    
    # Fix PlaceholderText (UWP/WinUI) to WPF equivalent
    $content = $content -replace '\s+PlaceholderText="([^"]*)"', ' Text="$1"'
    
    # Replace ProgressRing (UWP/WinUI) with ProgressBar (WPF)
    $content = $content -replace '<ProgressRing([^>]*)/>', '<ProgressBar IsIndeterminate="True"$1/>'
    $content = $content -replace '<ProgressRing([^>]*)>', '<ProgressBar IsIndeterminate="True"$1>'
    $content = $content -replace '</ProgressRing>', '</ProgressBar>'
    
    # Replace RadioButtons (UWP/WinUI) with RadioButton (WPF)
    $content = $content -replace '<RadioButtons([^>]*)>', '<StackPanel$1>'
    $content = $content -replace '</RadioButtons>', '</StackPanel>'
    
    # Fix TabControlItem to TabItem (correct WPF syntax)
    $content = $content -replace '<TabControlItem([^>]*)>', '<TabItem$1>'
    $content = $content -replace '</TabControlItem>', '</TabItem>'
    
    # Fix Target property in animations (UWP/WinUI syntax)
    $content = $content -replace '\s+Target="[^"]*"', ''
    
    # Remove BorderThickness from StackPanel if it exists
    $content = $content -replace '(<StackPanel[^>]*)\s+BorderThickness="[^"]*"([^>]*>)', '$1$2'
    
    # Remove CornerRadius from StackPanel if it exists
    $content = $content -replace '(<StackPanel[^>]*)\s+CornerRadius="[^"]*"([^>]*>)', '$1$2'
    
    if ($content -ne $originalContent) {
        Set-Content -Path $file.FullName -Value $content -Encoding UTF8
        Write-Host "  ✓ Fixed second wave compatibility issues" -ForegroundColor Green
    } else {
        Write-Host "  - No changes needed" -ForegroundColor Gray
    }
}

Write-Host "`nSecond wave XAML compatibility fixes completed!" -ForegroundColor Green
Write-Host "Run 'dotnet clean && dotnet build' to test the fixes." -ForegroundColor Yellow
