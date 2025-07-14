# Fix AccessibilitySettingsPage.xaml - Replace Spacing with Margin
$accessibilityPage = "Smartitecture\Application\UI\AccessibilitySettingsPage.xaml"
(Get-Content $accessibilityPage -Raw) -replace 'Spacing="(\d+)"', 'Margin="0,0,0,$1"' | Set-Content $accessibilityPage -NoNewline

# Fix AgentPage.xaml - Fix DataType and converter namespace
$agentPage = "Smartitecture\Application\UI\AgentPage.xaml"
$agentContent = Get-Content $agentPage -Raw
$agentContent = $agentContent -replace 'xmlns:converters="using:Smartitecture\.UI\.Converters"', 'xmlns:converters="clr-namespace:Smartitecture.Application.UI.Converters"'
$agentContent = $agentContent -replace 'DataType="viewmodels:ChatMessageViewModel"', 'DataContext="{Binding}"'
$agentContent | Set-Content $agentPage -NoNewline

# Fix ChatPage.xaml - Replace x:Bind with Binding
$chatPage = "Smartitecture\Application\UI\ChatPage.xaml"
(Get-Content $chatPage -Raw) -replace 'x:Bind', 'Binding' | Set-Content $chatPage -NoNewline

# Fix GettingStartedPage.xaml - Replace Spacing with Margin
$gettingStartedPage = "Smartitecture\Application\UI\GettingStartedPage.xaml"
(Get-Content $gettingStartedPage -Raw) -replace 'Spacing="(\d+)"', 'Margin="0,0,0,$1"' | Set-Content $gettingStartedPage -NoNewline

# Fix ScreenAnalysisPage.xaml - Replace InfoBar with a WPF alternative
$screenAnalysisPage = "Smartitecture\Application\UI\ScreenAnalysisPage.xaml"
$screenAnalysisContent = Get-Content $screenAnalysisPage -Raw
$screenAnalysisContent = $screenAnalysisContent -replace '<InfoBar\s+[^>]*>', '<Border Background="#E6F2FF" BorderBrush="#99C2FF" BorderThickness="1" CornerRadius="4" Padding="12" Margin="0,0,0,16"><StackPanel>'
$screenAnalysisContent = $screenAnalysisContent -replace '</InfoBar>', '</StackPanel></Border>'
$screenAnalysisContent = $screenAnalysisContent -replace 'IsOpen="[^"]*"', ''
$screenAnalysisContent = $screenAnalysisContent -replace 'Severity="[^"]*"', ''
$screenAnalysisContent = $screenAnalysisContent -replace 'Title="([^"]*)"', '<TextBlock FontWeight="Bold" Margin="0,0,0,4">$1</TextBlock>'
$screenAnalysisContent | Set-Content $screenAnalysisPage -NoNewline

# Fix SecurityPage.xaml - Replace Pivot with TabControl
$securityPage = "Smartitecture\Application\UI\SecurityPage.xaml"
$securityContent = Get-Content $securityPage -Raw
$securityContent = $securityContent -replace '<Pivot>', '<TabControl>'
$securityContent = $securityContent -replace '</Pivot>', '</TabControl>'
$securityContent = $securityContent -replace '<PivotItem\s+Header="([^"]*)">', '<TabItem Header="$1"><Grid Margin="8">'
$securityContent = $securityContent -replace '</PivotItem>', '</Grid></TabItem>'
$securityContent | Set-Content $securityPage -NoNewline

Write-Host "XAML files have been updated. Please build the solution again."
