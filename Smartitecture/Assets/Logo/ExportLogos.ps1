# PowerShell script to export the AIPal logo in various sizes
# This script uses the System.Windows.Media namespace to render the XAML logo to PNG files

Add-Type -AssemblyName PresentationCore
Add-Type -AssemblyName PresentationFramework
Add-Type -AssemblyName WindowsBase

# Sizes needed for the application package
$sizes = @(
    @{Name = "Logo44x44.png"; Width = 44; Height = 44},
    @{Name = "Logo50x50.png"; Width = 50; Height = 50},
    @{Name = "Logo71x71.png"; Width = 71; Height = 71},
    @{Name = "Logo150x150.png"; Width = 150; Height = 150},
    @{Name = "Logo310x310.png"; Width = 310; Height = 310},
    @{Name = "Logo620x300.png"; Width = 620; Height = 300},
    @{Name = "SplashScreen620x300.png"; Width = 620; Height = 300},
    @{Name = "StoreLogo.png"; Width = 50; Height = 50},
    @{Name = "BadgeLogo.png"; Width = 24; Height = 24},
    @{Name = "LargeTile310x310.png"; Width = 310; Height = 310},
    @{Name = "SmallTile71x71.png"; Width = 71; Height = 71},
    @{Name = "AppIcon.ico"; Width = 256; Height = 256}
)

# Function to render the logo to a PNG file
function Export-LogoToPng {
    param (
        [string]$OutputPath,
        [int]$Width,
        [int]$Height
    )
    
    # Create a simple window to host the rendering
    $window = New-Object System.Windows.Window
    $window.Width = $Width
    $window.Height = $Height
    $window.Visibility = [System.Windows.Visibility]::Hidden
    
    # Create an image control with the logo
    $image = New-Object System.Windows.Controls.Image
    $image.Width = $Width
    $image.Height = $Height
    
    # Use a simple blue robot face for the icon
    $image.Source = New-Object System.Windows.Media.DrawingImage
    
    # Create a drawing group for the robot face
    $drawingGroup = New-Object System.Windows.Media.DrawingGroup
    
    # Robot Head Background (Blue)
    $geometry = New-Object System.Windows.Media.PathGeometry
    $figure = New-Object System.Windows.Media.PathFigure
    $figure.StartPoint = New-Object System.Windows.Point(10, 6)
    $figure.Segments.Add((New-Object System.Windows.Media.ArcSegment(
        (New-Object System.Windows.Point(30, 6)),
        (New-Object System.Windows.Size(10, 10)),
        0, $false, [System.Windows.Media.SweepDirection]::Clockwise, $true)))
    $figure.Segments.Add((New-Object System.Windows.Media.LineSegment(
        (New-Object System.Windows.Point(30, 24)), $true)))
    $figure.Segments.Add((New-Object System.Windows.Media.LineSegment(
        (New-Object System.Windows.Point(24, 30)), $true)))
    $figure.Segments.Add((New-Object System.Windows.Media.LineSegment(
        (New-Object System.Windows.Point(16, 24)), $true)))
    $figure.Segments.Add((New-Object System.Windows.Media.LineSegment(
        (New-Object System.Windows.Point(10, 24)), $true)))
    $figure.Segments.Add((New-Object System.Windows.Media.ArcSegment(
        (New-Object System.Windows.Point(10, 6)),
        (New-Object System.Windows.Size(10, 10)),
        0, $false, [System.Windows.Media.SweepDirection]::Clockwise, $true)))
    $figure.IsClosed = $true
    $geometry.Figures.Add($figure)
    
    $drawing = New-Object System.Windows.Media.GeometryDrawing
    $drawing.Geometry = $geometry
    $drawing.Brush = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Color]::FromRgb(53, 132, 217))
    $drawingGroup.Children.Add($drawing)
    
    # Robot Face (Inner part - Lighter Blue)
    $geometry = New-Object System.Windows.Media.PathGeometry
    $figure = New-Object System.Windows.Media.PathFigure
    $figure.StartPoint = New-Object System.Windows.Point(12, 7)
    $figure.Segments.Add((New-Object System.Windows.Media.ArcSegment(
        (New-Object System.Windows.Point(28, 7)),
        (New-Object System.Windows.Size(8, 8)),
        0, $false, [System.Windows.Media.SweepDirection]::Clockwise, $true)))
    $figure.Segments.Add((New-Object System.Windows.Media.LineSegment(
        (New-Object System.Windows.Point(28, 22)), $true)))
    $figure.Segments.Add((New-Object System.Windows.Media.LineSegment(
        (New-Object System.Windows.Point(23, 27)), $true)))
    $figure.Segments.Add((New-Object System.Windows.Media.LineSegment(
        (New-Object System.Windows.Point(17, 22)), $true)))
    $figure.Segments.Add((New-Object System.Windows.Media.LineSegment(
        (New-Object System.Windows.Point(12, 22)), $true)))
    $figure.Segments.Add((New-Object System.Windows.Media.ArcSegment(
        (New-Object System.Windows.Point(12, 7)),
        (New-Object System.Windows.Size(8, 8)),
        0, $false, [System.Windows.Media.SweepDirection]::Clockwise, $true)))
    $figure.IsClosed = $true
    $geometry.Figures.Add($figure)
    
    $drawing = New-Object System.Windows.Media.GeometryDrawing
    $drawing.Geometry = $geometry
    $drawing.Brush = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Color]::FromRgb(77, 159, 243))
    $drawingGroup.Children.Add($drawing)
    
    # Left Antenna
    $geometry = New-Object System.Windows.Media.PathGeometry
    $figure = New-Object System.Windows.Media.PathFigure
    $figure.StartPoint = New-Object System.Windows.Point(14, 6)
    $figure.Segments.Add((New-Object System.Windows.Media.LineSegment(
        (New-Object System.Windows.Point(12, 2)), $true)))
    $figure.Segments.Add((New-Object System.Windows.Media.ArcSegment(
        (New-Object System.Windows.Point(16, 0)),
        (New-Object System.Windows.Size(2, 2)),
        0, $false, [System.Windows.Media.SweepDirection]::Clockwise, $true)))
    $figure.IsClosed = $true
    $geometry.Figures.Add($figure)
    
    $drawing = New-Object System.Windows.Media.GeometryDrawing
    $drawing.Geometry = $geometry
    $drawing.Brush = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Color]::FromRgb(53, 132, 217))
    $drawingGroup.Children.Add($drawing)
    
    # Right Antenna
    $geometry = New-Object System.Windows.Media.PathGeometry
    $figure = New-Object System.Windows.Media.PathFigure
    $figure.StartPoint = New-Object System.Windows.Point(26, 6)
    $figure.Segments.Add((New-Object System.Windows.Media.LineSegment(
        (New-Object System.Windows.Point(28, 2)), $true)))
    $figure.Segments.Add((New-Object System.Windows.Media.ArcSegment(
        (New-Object System.Windows.Point(24, 0)),
        (New-Object System.Windows.Size(2, 2)),
        0, $false, [System.Windows.Media.SweepDirection]::Clockwise, $true)))
    $figure.IsClosed = $true
    $geometry.Figures.Add($figure)
    
    $drawing = New-Object System.Windows.Media.GeometryDrawing
    $drawing.Geometry = $geometry
    $drawing.Brush = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Color]::FromRgb(53, 132, 217))
    $drawingGroup.Children.Add($drawing)
    
    # Left Eye
    $geometry = New-Object System.Windows.Media.EllipseGeometry(
        (New-Object System.Windows.Point(16, 12)),
        3, 3)
    $drawing = New-Object System.Windows.Media.GeometryDrawing
    $drawing.Geometry = $geometry
    $drawing.Brush = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Colors]::White)
    $drawingGroup.Children.Add($drawing)
    
    # Right Eye
    $geometry = New-Object System.Windows.Media.EllipseGeometry(
        (New-Object System.Windows.Point(24, 12)),
        3, 3)
    $drawing = New-Object System.Windows.Media.GeometryDrawing
    $drawing.Geometry = $geometry
    $drawing.Brush = New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Colors]::White)
    $drawingGroup.Children.Add($drawing)
    
    # Smile
    $geometry = New-Object System.Windows.Media.PathGeometry
    $figure = New-Object System.Windows.Media.PathFigure
    $figure.StartPoint = New-Object System.Windows.Point(14, 18)
    $figure.Segments.Add((New-Object System.Windows.Media.ArcSegment(
        (New-Object System.Windows.Point(26, 18)),
        (New-Object System.Windows.Size(6, 4)),
        0, $false, [System.Windows.Media.SweepDirection]::Clockwise, $true)))
    $figure.IsClosed = $false
    $geometry.Figures.Add($figure)
    
    $drawing = New-Object System.Windows.Media.GeometryDrawing
    $drawing.Geometry = $geometry
    $drawing.Pen = New-Object System.Windows.Media.Pen(
        (New-Object System.Windows.Media.SolidColorBrush([System.Windows.Media.Colors]::White)),
        1.6)
    $drawingGroup.Children.Add($drawing)
    
    $image.Source.Drawing = $drawingGroup
    
    $window.Content = $image
    $window.Show()
    
    # Render the image to a bitmap
    $renderTargetBitmap = New-Object System.Windows.Media.Imaging.RenderTargetBitmap(
        $Width, $Height, 96, 96, [System.Windows.Media.PixelFormats]::Pbgra32)
    $renderTargetBitmap.Render($image)
    
    # Create a PNG encoder
    $encoder = New-Object System.Windows.Media.Imaging.PngBitmapEncoder
    $encoder.Frames.Add([System.Windows.Media.Imaging.BitmapFrame]::Create($renderTargetBitmap))
    
    # Save the image to a file
    $stream = New-Object System.IO.FileStream($OutputPath, [System.IO.FileMode]::Create)
    $encoder.Save($stream)
    $stream.Close()
    
    $window.Close()
}

# Export the logo in various sizes
foreach ($size in $sizes) {
    $outputPath = Join-Path -Path $PSScriptRoot -ChildPath $size.Name
    Write-Host "Exporting $($size.Name) ($($size.Width)x$($size.Height))"
    Export-LogoToPng -OutputPath $outputPath -Width $size.Width -Height $size.Height
}

Write-Host "Logo export complete!"
