# Fix AccessibilitySettingsPage.xaml - Fix BorderBrush and other properties
$accessibilityPage = "Smartitecture\Application\UI\AccessibilitySettingsPage.xaml"
$accessibilityContent = Get-Content $accessibilityPage -Raw
$accessibilityContent = $accessibilityContent -replace 'BorderBrush="[^"]*"', 'BorderBrush="{DynamicResource SystemControlBackgroundBaseLowBrush}"'
$accessibilityContent | Set-Content $accessibilityPage -NoNewline

# Fix AgentPage.xaml - Fix DataContext binding
$agentPage = "Smartitecture\Application\UI\AgentPage.xaml"
$agentContent = Get-Content $agentPage -Raw
$agentContent = $agentContent -replace 'DataContext=\"\{Binding\}\"', ''
$agentContent | Set-Content $agentPage -NoNewline

# Fix ChatPage.xaml - Replace PersonPicture with WPF alternative
$chatPage = "Smartitecture\Application\UI\ChatPage.xaml"
$chatContent = Get-Content $chatPage -Raw
$chatContent = $chatContent -replace '<PersonPicture[^>]*>', '<Ellipse Width="40" Height="40" Fill="{StaticResource SystemControlBackgroundAccentBrush}">\n            <Ellipse.Style>\n                <Style TargetType="Ellipse">\n                    <Setter Property="Fill" Value="{StaticResource SystemControlBackgroundAccentBrush}" />\n                    <Style.Triggers>\n                        <DataTrigger Binding="{Binding IsUser}" Value="True">\n                            <Setter Property="Fill" Value="{StaticResource SystemAccentColor}" />\n                        </DataTrigger>\n                    </Style.Triggers>\n                </Style>\n            </Ellipse.Style>\n            <TextBlock Text="{Binding Initials}" Foreground="White" HorizontalAlignment="Center" VerticalAlignment="Center" FontWeight="SemiBold"/>\n        </Ellipse>'
$chatContent = $chatContent -replace '</PersonPicture>', ''
$chatContent | Set-Content $chatPage -NoNewline

# Fix GettingStartedPage.xaml - Fix BorderBrush
$gettingStartedPage = "Smartitecture\Application\UI\GettingStartedPage.xaml"
$gettingStartedContent = Get-Content $gettingStartedPage -Raw
$gettingStartedContent = $gettingStartedContent -replace 'BorderBrush="[^"]*"', 'BorderBrush="{DynamicResource SystemControlBackgroundBaseLowBrush}"'
$gettingStartedContent | Set-Content $gettingStartedPage -NoNewline

# Fix ScreenAnalysisPage.xaml - Replace Spacing with Margin
$screenAnalysisPage = "Smartitecture\Application\UI\ScreenAnalysisPage.xaml"
(Get-Content $screenAnalysisPage -Raw) -replace 'Spacing="(\d+)"', 'Margin="0,0,0,$1"' | Set-Content $screenAnalysisPage -NoNewline

# Fix SecurityPage.xaml - Replace Spacing with Margin
$securityPage = "Smartitecture\Application\UI\SecurityPage.xaml"
(Get-Content $securityPage -Raw) -replace 'Spacing="(\d+)"', 'Margin="0,0,0,$1"' | Set-Content $securityPage -NoNewline

Write-Host "XAML files have been updated. Please build the solution again."
