$files = Get-ChildItem -Path ".\Smartitecture\Application\UI\*.xaml" -Recurse

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    
    # Replace ThemeResource with StaticResource
    $content = $content -replace '\{ThemeResource\s*([^\}]+)\}', '{StaticResource $1}'
    
    # Save the updated content
    $content | Set-Content $file.FullName -NoNewline
    
    Write-Host "Updated $($file.Name)"
}

Write-Host "All XAML files have been updated."
