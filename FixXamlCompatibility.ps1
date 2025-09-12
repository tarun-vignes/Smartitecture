# PowerShell script to fix UWP/WinUI compatibility issues in XAML files for WPF
# This script will systematically replace UWP/WinUI-specific elements with WPF-compatible equivalents

Write-Host "Starting XAML compatibility fixes for WPF..." -ForegroundColor Green

# Define the target directory
$xamlPath = "c:\projects\Smartitecture\Smartitecture\Application\UI"
$themesPath = "c:\projects\Smartitecture\Smartitecture\Themes"

# Get all XAML files
$xamlFiles = Get-ChildItem -Path $xamlPath -Filter "*.xaml" -Recurse
$themeFiles = Get-ChildItem -Path $themesPath -Filter "*.xaml" -Recurse
$allFiles = $xamlFiles + $themeFiles

Write-Host "Found $($allFiles.Count) XAML files to process..." -ForegroundColor Yellow

foreach ($file in $allFiles) {
    Write-Host "Processing: $($file.Name)" -ForegroundColor Cyan
    
    $content = Get-Content $file.FullName -Raw
    $originalContent = $content
    
    # 1. Remove all Spacing properties from StackPanel elements
    $content = $content -replace '\s+Spacing="[^"]*"', ''
    
    # 2. Replace ThemeResource with static colors
    $content = $content -replace '\{ThemeResource\s+SystemAccentColor\}', '#FF0078D4'
    $content = $content -replace '\{ThemeResource\s+CardBackgroundFillColorDefaultBrush\}', '#FF2D2D30'
    $content = $content -replace '\{ThemeResource\s+CardStrokeColorDefaultBrush\}', '#FF3F3F46'
    $content = $content -replace '\{ThemeResource\s+SystemFillColorCriticalBackgroundBrush\}', '#FFFF4444'
    $content = $content -replace '\{ThemeResource\s+[^}]+\}', '#FF2D2D30'
    
    # 3. Replace x:Bind with Binding (WPF syntax)
    $content = $content -replace '\{x:Bind\s+([^}]+)\}', '{Binding $1}'
    
    # 4. Remove x:DataType properties (UWP/WinUI only)
    $content = $content -replace '\s+x:DataType="[^"]*"', ''
    
    # 5. Replace UWP-only controls with WPF equivalents
    
    # Replace NavigationView with TabControl
    $content = $content -replace '<NavigationView([^>]*)>', '<TabControl$1>'
    $content = $content -replace '</NavigationView>', '</TabControl>'
    
    # Replace Pivot with TabControl
    $content = $content -replace '<Pivot([^>]*)>', '<TabControl$1>'
    $content = $content -replace '</Pivot>', '</TabControl>'
    
    # Replace InfoBar with Border (already done manually, but just in case)
    $content = $content -replace '<InfoBar([^>]*)/>', '<Border Background="#FF0078D4" CornerRadius="4" Padding="12"><TextBlock Text="Information" Foreground="White"/></Border>'
    
    # Replace PersonPicture with Ellipse
    $content = $content -replace '<PersonPicture([^>]*)/>', '<Ellipse Width="32" Height="32"><Ellipse.Fill><ImageBrush Stretch="UniformToFill"/></Ellipse.Fill></Ellipse>'
    
    # 6. Fix VisualState.Setters (UWP syntax) to WPF syntax
    $content = $content -replace '<VisualState\.Setters>', '<VisualState><Storyboard>'
    $content = $content -replace '</VisualState\.Setters>', '</Storyboard></VisualState>'
    
    # 7. Remove XamlControlsResources (UWP/WinUI only)
    $content = $content -replace '<XamlControlsResources[^>]*/?>', '<!-- XamlControlsResources removed for WPF compatibility -->'
    
    # 8. Fix namespace declarations - replace UWP 'using:' with WPF 'clr-namespace:'
    $content = $content -replace 'xmlns:([^=]+)="using:([^"]+)"', 'xmlns:$1="clr-namespace:$2"'
    
    # 9. Remove RequestedTheme property (UWP/WinUI only)
    $content = $content -replace '\s+RequestedTheme="[^"]*"', ''
    
    # 10. Fix FontIcon to TextBlock with Unicode (if any remain)
    $content = $content -replace '<FontIcon\s+Glyph="([^"]*)"([^>]*)/>', '<TextBlock Text="$1"$2/>'
    
    # 11. Remove CornerRadius from Button (WPF Button doesn't support it directly)
    $content = $content -replace '(<Button[^>]*)\s+CornerRadius="[^"]*"([^>]*>)', '$1$2'
    
    # 12. Replace Padding with Margin where Padding doesn't exist (like on Grid)
    # This is tricky, so we'll be conservative and only fix known cases
    $content = $content -replace '(<Grid[^>]*)\s+Padding="([^"]*)"([^>]*>)', '$1 Margin="$2"$3'
    
    # Only write if content changed
    if ($content -ne $originalContent) {
        Set-Content -Path $file.FullName -Value $content -Encoding UTF8
        Write-Host "  ✓ Fixed UWP/WinUI compatibility issues" -ForegroundColor Green
    } else {
        Write-Host "  - No changes needed" -ForegroundColor Gray
    }
}

Write-Host "`nXAML compatibility fixes completed!" -ForegroundColor Green
Write-Host "Run 'dotnet clean && dotnet build' to test the fixes." -ForegroundColor Yellow
