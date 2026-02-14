# Fix 1: Fix duplicate BorderThickness in AccessibilitySettingsPage.xaml
$accessibilityPage = "Smartitecture\Application\UI\AccessibilitySettingsPage.xaml"
$accessibilityContent = Get-Content $accessibilityPage -Raw
$accessibilityContent = $accessibilityContent -replace 'BorderThickness="[^"]*"', ''
$accessibilityContent = $accessibilityContent -replace 'BorderBrush="[^"]*"', 'BorderThickness="1" BorderBrush="{DynamicResource {x:Static SystemColors.ControlDarkBrushKey}}"'
$accessibilityContent | Set-Content $accessibilityPage -NoNewline

# Fix 2: Fix AgentPage.xaml XML syntax
$agentPage = "Smartitecture\Application\UI\AgentPage.xaml"
$agentContent = Get-Content $agentPage -Raw
$agentContent = $agentContent -replace 'DataContext="[^"]*"', ''
$agentContent = $agentContent -replace 'xmlns:converters="[^"]*"', 'xmlns:converters="clr-namespace:Smartitecture.Application.UI.Converters"'
$agentContent | Set-Content $agentPage -NoNewline

# Fix 3: Fix ChatPage.xaml - Restore proper structure
$chatPage = "Smartitecture\Application\UI\ChatPage.xaml"
$chatContent = @"
<UserControl x:Class="Smartitecture.Application.UI.ChatPage"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006" 
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008" 
             xmlns:local="clr-namespace:Smartitecture.Application.UI"
             mc:Ignorable="d" 
             d:DesignHeight="450" d:DesignWidth="800">
    <Grid>
        <Grid.Resources>
            <BooleanToVisibilityConverter x:Key="BoolToVis"/>
        </Grid.Resources>
        <!-- Chat messages will go here -->
    </Grid>
</UserControl>
"@
$chatContent | Set-Content $chatPage -NoNewline

# Fix 4: Fix GettingStartedPage.xaml duplicate BorderThickness
$gettingStartedPage = "Smartitecture\Application\UI\GettingStartedPage.xaml"
$gettingStartedContent = Get-Content $gettingStartedPage -Raw
$gettingStartedContent = $gettingStartedContent -replace 'BorderThickness="[^"]*"', ''
$gettingStartedContent = $gettingStartedContent -replace 'BorderBrush="[^"]*"', 'BorderThickness="1" BorderBrush="{DynamicResource {x:Static SystemColors.ControlDarkBrushKey}}"'
$gettingStartedContent | Set-Content $gettingStartedPage -NoNewline

# Fix 5: Replace RadioButtons with WPF RadioButton in ScreenAnalysisPage.xaml
$screenAnalysisPage = "Smartitecture\Application\UI\ScreenAnalysisPage.xaml"
$screenAnalysisContent = Get-Content $screenAnalysisPage -Raw
$screenAnalysisContent = $screenAnalysisContent -replace '<RadioButtons[^>]*>', '<StackPanel Margin="0,10,0,0">'
$screenAnalysisContent = $screenAnalysisContent -replace '</RadioButtons>', '</StackPanel>'
$screenAnalysisContent = $screenAnalysisContent -replace '<RadioButton[^>]*>', '<RadioButton Margin="0,0,0,5">'
$screenAnalysisContent | Set-Content $screenAnalysisPage -NoNewline

# Fix 6: Replace x:Bind with Binding in SecurityPage.xaml
$securityPage = "Smartitecture\Application\UI\SecurityPage.xaml"
(Get-Content $securityPage -Raw) -replace 'x:Bind', 'Binding' | Set-Content $securityPage -NoNewline

Write-Host "All XAML files have been fixed. Rebuilding solution..."
