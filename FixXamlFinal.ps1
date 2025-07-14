# Fix 1: Replace BorderBrush with a WPF-compatible alternative in AccessibilitySettingsPage.xaml
$accessibilityPage = "Smartitecture\Application\UI\AccessibilitySettingsPage.xaml"
(Get-Content $accessibilityPage -Raw) -replace 'BorderBrush="[^"]*"', 'BorderThickness="1" BorderBrush="{DynamicResource {x:Static SystemColors.ControlDarkBrushKey}}"' | Set-Content $accessibilityPage -NoNewline

# Fix 2: Fix AgentPage.xaml XML syntax error
$agentPage = "Smartitecture\Application\UI\AgentPage.xaml"
$agentContent = Get-Content $agentPage -Raw
$agentContent = $agentContent -replace 'DataContext="[^"]*"', ''
$agentContent | Set-Content $agentPage -NoNewline

# Fix 3: Fix ChatPage.xaml Ellipse definition
$chatPage = "Smartitecture\Application\UI\ChatPage.xaml"
$chatContent = @"
<Grid>
    <Grid.Resources>
        <BooleanToVisibilityConverter x:Key="BoolToVis"/>
    </Grid.Resources>
    <Ellipse Width="40" Height="40" Fill="#0078D7">
        <Ellipse.Style>
            <Style TargetType="Ellipse">
                <Setter Property="Fill" Value="#0078D7"/>
                <Style.Triggers>
                    <DataTrigger Binding="{Binding IsUser}" Value="True">
                        <Setter Property="Fill" Value="#E81123"/>
                    </DataTrigger>
                </Style.Triggers>
            </Style>
        </Ellipse.Style>
        <TextBlock Text="{Binding Initials}" 
                   Foreground="White" 
                   HorizontalAlignment="Center" 
                   VerticalAlignment="Center" 
                   FontWeight="SemiBold"/>
    </Ellipse>
</Grid>
"@
$chatContent | Set-Content $chatPage -NoNewline

# Fix 4: Fix GettingStartedPage.xaml BorderBrush
$gettingStartedPage = "Smartitecture\Application\UI\GettingStartedPage.xaml"
(Get-Content $gettingStartedPage -Raw) -replace 'BorderBrush="[^"]*"', 'BorderThickness="1" BorderBrush="{DynamicResource {x:Static SystemColors.ControlDarkBrushKey}}"' | Set-Content $gettingStartedPage -NoNewline

# Fix 5: Fix ScreenAnalysisPage.xaml duplicate Margin
$screenAnalysisPage = "Smartitecture\Application\UI\ScreenAnalysisPage.xaml"
(Get-Content $screenAnalysisPage -Raw) -replace 'Margin="[^"]*"', '' | Set-Content $screenAnalysisPage -NoNewline

# Fix 6: Replace InfoBar in SecurityPage.xaml with a WPF alternative
$securityPage = "Smartitecture\Application\UI\SecurityPage.xaml"
$securityContent = Get-Content $securityPage -Raw
$securityContent = $securityContent -replace '<InfoBar[^>]*>', '<Border Background="#E6F2FF" BorderBrush="#99C2FF" BorderThickness="1" CornerRadius="4" Padding="12" Margin="0,0,0,16"><StackPanel>'
$securityContent = $securityContent -replace '</InfoBar>', '</StackPanel></Border>'
$securityContent = $securityContent -replace 'IsOpen="[^"]*"', ''
$securityContent = $securityContent -replace 'Severity="[^"]*"', ''
$securityContent = $securityContent -replace 'Title="([^"]*)"', '<TextBlock FontWeight="Bold" Margin="0,0,0,4">$1</TextBlock>'
$securityContent | Set-Content $securityPage -NoNewline

Write-Host "Final XAML fixes applied. Please rebuild the solution."
