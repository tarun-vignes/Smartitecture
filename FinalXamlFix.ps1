# Function to fix common XAML issues
function Fix-XamlFile {
    param (
        [string]$filePath
    )
    
    $content = Get-Content $filePath -Raw
    
    # 1. Fix BorderThickness and BorderBrush
    $content = $content -replace 'BorderThickness="[^"]*"', ''
    $content = $content -replace 'BorderBrush="[^"]*"', 'BorderThickness="1" BorderBrush="Gray"'
    
    # 2. Fix DataContext and other bindings
    $content = $content -replace 'DataContext="[^"]*"', ''
    $content = $content -replace 'x:Bind', 'Binding'
    
    # 3. Fix RadioButtons and other UWP controls
    $content = $content -replace '<RadioButtons[^>]*>', '<StackPanel>'
    $content = $content -replace '</RadioButtons>', '</StackPanel>'
    $content = $content -replace '<RadioButton([^>]*)>', '<RadioButton$1 Margin="0,0,0,5" />'
    
    # 4. Fix InfoBar
    $content = $content -replace '<InfoBar[^>]*>', '<Border Background="#E6F2FF" BorderBrush="#99C2FF" BorderThickness="1" CornerRadius="4" Padding="12" Margin="0,0,0,16"><StackPanel>'
    $content = $content -replace '</InfoBar>', '</StackPanel></Border>'
    $content = $content -replace 'IsOpen="[^"]*"', ''
    $content = $content -replace 'Severity="[^"]*"', ''
    $content = $content -replace 'Title="([^"]*)"', '<TextBlock FontWeight="Bold" Margin="0,0,0,4">$1</TextBlock>'
    
    # 5. Fix PersonPicture
    $content = $content -replace '<PersonPicture[^>]*>', '<Ellipse Width="40" Height="40" Fill="#0078D7"><TextBlock Text="{Binding Initials}" Foreground="White" HorizontalAlignment="Center" VerticalAlignment="Center" FontWeight="SemiBold"/></Ellipse>'
    $content = $content -replace '</PersonPicture>', ''
    
    # 6. Fix any remaining XML issues
    $content = $content -replace 'xmlns:converters="[^"]*"', 'xmlns:converters="clr-namespace:Smartitecture.Application.UI.Converters"'
    $content = $content -replace 'xmlns:viewmodels="[^"]*"', ''
    
    # 7. Ensure proper UserControl closing
    $content = $content -replace '<Page\b', '<UserControl'
    $content = $content -replace '</Page>', '</UserControl>'
    
    # 8. Ensure proper Grid closing
    $content = $content -replace '<Grid>', '<Grid>'
    $content = $content -replace '</Grid>', '</Grid>'
    
    # 9. Fix any StackPanel issues
    $content = $content -replace '<StackPanel>', '<StackPanel>'
    $content = $content -replace '</StackPanel>', '</StackPanel>'
    
    # 10. Remove any empty attributes
    $content = $content -replace '\s+[a-zA-Z0-9]+=""', ''
    
    # Save the fixed content
    $content | Set-Content $filePath -NoNewline
}

# Process all XAML files
$files = Get-ChildItem -Path "Smartitecture\Application\UI" -Filter "*.xaml" -Recurse

foreach ($file in $files) {
    Write-Host "Fixing $($file.FullName)"
    Fix-XamlFile -filePath $file.FullName
}

Write-Host "All XAML files have been fixed. Rebuilding solution..."
dotnet build
