# Function to safely load and save XML content
function Fix-XmlFile {
    param (
        [string]$filePath
    )
    
    try {
        # Read the file content
        $content = [System.IO.File]::ReadAllText($filePath)
        
        # Fix common XAML issues
        $content = $content -replace 'BorderThickness="[^"]*"', ''
        $content = $content -replace 'BorderBrush="[^"]*"', 'BorderThickness="1" BorderBrush="Gray"'
        $content = $content -replace 'DataContext="[^"]*"', ''
        $content = $content -replace 'x:Bind', 'Binding'
        $content = $content -replace 'xmlns:converters="[^"]*"', 'xmlns:converters="clr-namespace:Smartitecture.Application.UI.Converters"'
        $content = $content -replace 'xmlns:viewmodels="[^"]*"', ''
        $content = $content -replace '<Page\b', '<UserControl'
        $content = $content -replace '</Page>', '</UserControl>'
        $content = $content -replace '<RadioButtons[^>]*>', '<StackPanel>'
        $content = $content -replace '</RadioButtons>', '</StackPanel>'
        $content = $content -replace '<RadioButton([^>]*)>', '<RadioButton$1 Margin="0,0,0,5" />'
        $content = $content -replace '<InfoBar[^>]*>', '<Border Background="#E6F2FF" BorderBrush="#99C2FF" BorderThickness="1" CornerRadius="4" Padding="12" Margin="0,0,0,16"><StackPanel>'
        $content = $content -replace '</InfoBar>', '</StackPanel></Border>'
        $content = $content -replace 'IsOpen="[^"]*"', ''
        $content = $content -replace 'Severity="[^"]*"', ''
        $content = $content -replace 'Title="([^"]*)"', '<TextBlock FontWeight="Bold" Margin="0,0,0,4">$1</TextBlock>'
        $content = $content -replace '<PersonPicture[^>]*>', '<Ellipse Width="40" Height="40" Fill="#0078D7"><TextBlock Text="{Binding Initials}" Foreground="White" HorizontalAlignment="Center" VerticalAlignment="Center" FontWeight="SemiBold"/></Ellipse>'
        $content = $content -replace '</PersonPicture>', ''
        
        # Fix any malformed XML
        $content = $content -replace '\s+[a-zA-Z0-9]+=""', ''
        $content = $content -replace '\s+[a-zA-Z0-9]+=\s*\"\s*"', ''
        $content = $content -replace '\s+[a-zA-Z0-9]+=\s*\"[^\"]*\"\s+[a-zA-Z0-9]+=\s*\"[^\"]*\"', ''
        
        # Ensure proper XML structure
        $content = $content -replace '>\s+<', '><'
        $content = $content -replace '\s+', ' '
        $content = $content -replace '> ', '>'
        $content = $content -replace ' <', '<'
        
        # Fix specific MainWindow.xaml issues
        if ($filePath -like '*MainWindow.xaml') {
            $content = '<Window x:Class="Smartitecture.Application.UI.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:local="clr-namespace:Smartitecture.Application.UI"
        mc:Ignorable="d"
        Title="Smartitecture" Height="768" Width="1024">
    <Grid>
        <Frame x:Name="MainFrame" NavigationUIVisibility="Hidden"/>
    </Grid>
</Window>'
        }
        
        # Save the fixed content
        [System.IO.File]::WriteAllText($filePath, $content.Trim())
        
        # Validate XML
        $xml = New-Object System.Xml.XmlDocument
        $xml.Load($filePath)
        
        Write-Host "Fixed: $filePath" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "Error fixing $filePath : $_" -ForegroundColor Red
        return $false
    }
}

# Process all XAML files
$files = Get-ChildItem -Path "Smartitecture\Application\UI" -Filter "*.xaml" -Recurse

foreach ($file in $files) {
    Fix-XmlFile -filePath $file.FullName
}

# Build the solution
Write-Host "`nBuilding solution..." -ForegroundColor Cyan
$buildOutput = dotnet build

if ($LASTEXITCODE -eq 0) {
    Write-Host "`nBuild successful! Starting application..." -ForegroundColor Green
    Start-Process -FilePath "dotnet" -ArgumentList "run --project Smartitecture\Smartitecture.csproj" -NoNewWindow
}
else {
    Write-Host "`nBuild failed. Please check the errors above." -ForegroundColor Red
    $buildOutput
}
