# PowerShell script to fix the final 3 UWP/WinUI compatibility issues
# These are the last remaining errors!

Write-Host "Starting FINAL wave of XAML compatibility fixes..." -ForegroundColor Green

$xamlPath = "c:\projects\Smartitecture\Smartitecture\Application\UI"
$themesPath = "c:\projects\Smartitecture\Smartitecture\Themes"

$xamlFiles = Get-ChildItem -Path $xamlPath -Filter "*.xaml" -Recurse
$themeFiles = Get-ChildItem -Path $themesPath -Filter "*.xaml" -Recurse
$allFiles = $xamlFiles + $themeFiles

Write-Host "Processing $($allFiles.Count) XAML files for FINAL fixes..." -ForegroundColor Yellow

foreach ($file in $allFiles) {
    Write-Host "Processing: $($file.Name)" -ForegroundColor Cyan
    
    $content = Get-Content $file.FullName -Raw
    $originalContent = $content
    
    # Fix Header property on wrong elements (probably should be Content or remove)
    $content = $content -replace '(<(?!Expander|GroupBox|TabItem)[^>]*)\s+Header="([^"]*)"([^>]*>)', '$1 Content="$2"$3'
    
    # Replace ToggleSwitch (UWP/WinUI) with CheckBox (WPF)
    $content = $content -replace '<ToggleSwitch([^>]*)/>', '<CheckBox$1/>'
    $content = $content -replace '<ToggleSwitch([^>]*)>', '<CheckBox$1>'
    $content = $content -replace '</ToggleSwitch>', '</CheckBox>'
    
    # Remove AutomationProperties.AccessibilityView (UWP/WinUI property)
    $content = $content -replace '\s+AutomationProperties\.AccessibilityView="[^"]*"', ''
    
    # Additional UWP/WinUI automation properties cleanup
    $content = $content -replace '\s+AutomationProperties\.Name="[^"]*"', ''
    $content = $content -replace '\s+AutomationProperties\.AutomationId="[^"]*"', ''
    
    if ($content -ne $originalContent) {
        Set-Content -Path $file.FullName -Value $content -Encoding UTF8
        Write-Host "  ✓ Fixed FINAL compatibility issues" -ForegroundColor Green
    } else {
        Write-Host "  - No changes needed" -ForegroundColor Gray
    }
}

Write-Host "`n🎉 FINAL XAML compatibility fixes completed! 🎉" -ForegroundColor Green
Write-Host "Run 'dotnet clean && dotnet build' to test for SUCCESS!" -ForegroundColor Yellow
