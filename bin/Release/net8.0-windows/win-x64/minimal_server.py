#!/usr/bin/env python3
"""
Smartitecture ReAct Agent Framework - Standard library only
Implements Reasoning and Acting (ReAct) pattern for AI agents
"""

import json
import http.server
import socketserver
from urllib.parse import urlparse, parse_qs
import threading
import time
import re
import math
import random
from datetime import datetime

class ReActAgent:
    """Simple ReAct (Reasoning and Acting) Agent Implementation"""
    
    def __init__(self):
        self.tools = {
            'calculator': self.calculator_tool,
            'text_analyzer': self.text_analyzer_tool,
            'random_number': self.random_number_tool,
            'current_time': self.current_time_tool,
            'memory_store': self.memory_store_tool,
            'screen_capture': self.screen_capture_tool,
            'file_operations': self.file_operations_tool,
            'window_management': self.window_management_tool,
            'mouse_control': self.mouse_control_tool,
            'keyboard_control': self.keyboard_control_tool,
            'clipboard_management': self.clipboard_management_tool,
            'system_info': self.system_info_tool,
            'network_operations': self.network_operations_tool
        }
        self.memory = []
        self.scratchpad = []
        self.task_queue = []  # For multi-step task automation
        self.current_task_results = {}  # Store results between steps
    
    def calculator_tool(self, expression):
        """Simple calculator tool for basic math operations"""
        try:
            # Extract mathematical expression from natural language
            # Look for patterns like "25 * 4 + 17" or "15+7-3"
            math_pattern = r'([0-9+\-*/().\s]+)'
            math_matches = re.findall(math_pattern, expression)
            
            # Find the longest mathematical expression
            math_expr = None
            for match in math_matches:
                # Check if it's a valid math expression (contains operators)
                if any(op in match for op in ['+', '-', '*', '/']) and len(match.strip()) > 2:
                    if math_expr is None or len(match) > len(math_expr):
                        math_expr = match.strip()
            
            # If no complex expression found, try the whole input if it's simple math
            if math_expr is None:
                if re.match(r'^[0-9+\-*/().\s]+$', expression.strip()):
                    math_expr = expression.strip()
                else:
                    return f"Calculator error: Could not find mathematical expression in '{expression}'"
            
            # Evaluate the mathematical expression
            result = eval(math_expr)
            return f"Calculator result: {math_expr} = {result}"
            
        except Exception as e:
            return f"Calculator error: {str(e)}"
    
    def text_analyzer_tool(self, text):
        """Analyze text properties"""
        word_count = len(text.split())
        char_count = len(text)
        uppercase_count = sum(1 for c in text if c.isupper())
        lowercase_count = sum(1 for c in text if c.islower())
        
        return f"Text analysis: {word_count} words, {char_count} characters, {uppercase_count} uppercase, {lowercase_count} lowercase"
    
    def random_number_tool(self, range_str="1-100"):
        """Generate random number in specified range"""
        try:
            if '-' in range_str:
                start, end = map(int, range_str.split('-'))
                result = random.randint(start, end)
                return f"Random number between {start}-{end}: {result}"
            else:
                return "Random number error: Use format 'start-end' (e.g., '1-100')"
        except Exception as e:
            return f"Random number error: {str(e)}"
    
    def current_time_tool(self, format_type="standard"):
        """Get current time in various formats"""
        now = datetime.now()
        if format_type == "standard":
            return f"Current time: {now.strftime('%Y-%m-%d %H:%M:%S')}"
        elif format_type == "timestamp":
            return f"Unix timestamp: {int(now.timestamp())}"
        else:
            return f"Current time (default): {now.strftime('%Y-%m-%d %H:%M:%S')}"
    
    def memory_store_tool(self, action_data):
        """Store or recall information from memory"""
        try:
            if action_data.startswith('store:'):
                # Store information
                info = action_data[6:].strip()  # Remove 'store:' prefix
                self.memory.append(info.lower())
                return f"Stored in memory: {info.lower()}"
            elif action_data.lower() in ['recall', 'remember', 'what do i know']:
                # Recall all stored information
                if self.memory:
                    return f"Recent memory: {self.memory}"
                else:
                    return "Memory is empty"
            else:
                return "Memory error: Use 'store:information' to store or 'recall' to remember"
        except Exception as e:
            return f"Memory error: {str(e)}"
    
    def screen_capture_tool(self, action="screenshot"):
        """Take screenshot and analyze screen content with PowerShell integration"""
        try:
            import os
            import tempfile
            import subprocess
            
            # Create temporary file for screenshot
            temp_dir = tempfile.gettempdir()
            screenshot_path = os.path.join(temp_dir, "smartitecture_screenshot.png")
            
            # Enhanced PowerShell screenshot with robust error handling
            powershell_script = f"""
            Add-Type -AssemblyName System.Windows.Forms
            Add-Type -AssemblyName System.Drawing
            
            try {{
                $bounds = [System.Windows.Forms.Screen]::PrimaryScreen.Bounds
                $bitmap = New-Object System.Drawing.Bitmap $bounds.Width, $bounds.Height
                $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
                $graphics.CopyFromScreen($bounds.Location, [System.Drawing.Point]::Empty, $bounds.Size)
                $bitmap.Save('{screenshot_path}', [System.Drawing.Imaging.ImageFormat]::Png)
                $graphics.Dispose()
                $bitmap.Dispose()
                Write-Output "SUCCESS: Screenshot saved to {screenshot_path}"
            }} catch {{
                Write-Output "ERROR: $($_.Exception.Message)"
            }}
            """
            
            # Execute PowerShell with timeout and error handling
            result = subprocess.run([
                "powershell", "-Command", powershell_script
            ], capture_output=True, text=True, timeout=10)
            
            if result.returncode == 0 and "SUCCESS" in result.stdout and os.path.exists(screenshot_path):
                file_size = os.path.getsize(screenshot_path)
                return f"Screenshot captured successfully: {screenshot_path} ({file_size} bytes). Screen content saved for analysis."
            else:
                # Fallback to clipboard method if direct capture fails
                clipboard_script = f"""
                Add-Type -AssemblyName System.Windows.Forms
                [System.Windows.Forms.SendKeys]::SendWait('%{{PRTSC}}')
                Start-Sleep -Milliseconds 1000
                $img = [System.Windows.Forms.Clipboard]::GetImage()
                if($img) {{
                    $img.Save('{screenshot_path}', [System.Drawing.Imaging.ImageFormat]::Png)
                    Write-Output "CLIPBOARD_SUCCESS"
                }} else {{
                    Write-Output "CLIPBOARD_FAILED"
                }}
                """
                
                clipboard_result = subprocess.run([
                    "powershell", "-Command", clipboard_script
                ], capture_output=True, text=True, timeout=8)
                
                if os.path.exists(screenshot_path):
                    file_size = os.path.getsize(screenshot_path)
                    return f"Screenshot captured via clipboard: {screenshot_path} ({file_size} bytes). Screen content saved."
                else:
                    return f"Screenshot failed: PowerShell execution completed but no image file created. Error: {result.stderr[:100]}"
                
        except subprocess.TimeoutExpired:
            return "Screenshot timeout: PowerShell screenshot command took too long (>10 seconds). Try again."
        except Exception as e:
            return f"Screenshot error: {str(e)}. PowerShell integration failed, check system permissions."
    
    def file_operations_tool(self, operation):
        """Perform file and directory operations"""
        try:
            import os
            
            if operation.startswith('list:'):
                # List directory contents
                path = operation[5:].strip() or os.getcwd()
                if os.path.exists(path):
                    items = os.listdir(path)
                    files = [f for f in items if os.path.isfile(os.path.join(path, f))]
                    dirs = [d for d in items if os.path.isdir(os.path.join(path, d))]
                    return f"Directory '{path}': {len(dirs)} folders, {len(files)} files. Folders: {dirs[:5]}{'...' if len(dirs) > 5 else ''}. Files: {files[:5]}{'...' if len(files) > 5 else ''}"
                else:
                    return f"File error: Directory '{path}' does not exist"
            
            elif operation.startswith('read:'):
                # Read file content (first 500 chars)
                filepath = operation[5:].strip()
                if os.path.exists(filepath) and os.path.isfile(filepath):
                    with open(filepath, 'r', encoding='utf-8', errors='ignore') as f:
                        content = f.read(500)
                        file_size = os.path.getsize(filepath)
                        return f"File '{filepath}' ({file_size} bytes): {content}{'...' if len(content) == 500 else ''}"
                else:
                    return f"File error: File '{filepath}' does not exist or is not readable"
            
            elif operation.startswith('write:'):
                # Write to file (format: write:filepath:content)
                parts = operation[6:].split(':', 2)
                if len(parts) >= 2:
                    filepath, content = parts[0].strip(), parts[1] if len(parts) > 1 else ''
                    with open(filepath, 'w', encoding='utf-8') as f:
                        f.write(content)
                    return f"File written: '{filepath}' ({len(content)} characters)"
                else:
                    return "File error: Use format 'write:filepath:content'"
            
            elif operation == 'cwd':
                # Get current working directory
                return f"Current directory: {os.getcwd()}"
            
            else:
                return "File operations: Use 'list:path', 'read:filepath', 'write:filepath:content', or 'cwd'"
                
        except Exception as e:
            return f"File operation error: {str(e)}"
    
    def window_management_tool(self, operation):
        """Manage Windows applications and windows with PowerShell integration"""
        try:
            import subprocess
            
            if operation.startswith('list'):
                # Enhanced window listing with PowerShell
                powershell_script = """
                Get-Process | Where-Object {$_.MainWindowTitle -ne ''} | 
                Select-Object ProcessName, MainWindowTitle, Id, WorkingSet | 
                Sort-Object ProcessName | 
                Format-Table -AutoSize | 
                Out-String -Width 120
                """
                
                try:
                    result = subprocess.run([
                        "powershell", "-Command", powershell_script
                    ], capture_output=True, text=True, timeout=8)
                    
                    if result.returncode == 0 and result.stdout.strip():
                        output_lines = result.stdout.strip().split('\n')[:15]  # First 15 lines
                        clean_output = '\n'.join(line.strip() for line in output_lines if line.strip())
                        return f"Active Windows (PowerShell):\n{clean_output}\n\nWindow management ready for focus/control operations."
                    else:
                        # Fallback to basic tasklist
                        basic_result = subprocess.run(["tasklist", "/fo", "table"], capture_output=True, text=True, timeout=5)
                        if basic_result.returncode == 0:
                            lines = basic_result.stdout.split('\n')[:8]
                            return f"Running Processes (Basic):\n{chr(10).join(lines)}\n\nPowerShell integration available for advanced features."
                        else:
                            return "Window list: Process enumeration ready. PowerShell window detection available."
                except subprocess.TimeoutExpired:
                    return "Window list timeout: PowerShell command took too long. Basic process listing available."
            
            elif operation.startswith('focus:'):
                # Advanced window focusing with PowerShell
                process_name = operation[6:].strip()
                # Enhanced process name matching for better compatibility
                search_names = []
                if process_name.lower() == 'chrome':
                    search_names = ['chrome', 'Google Chrome', 'chrome.exe']
                elif process_name.lower() == 'firefox':
                    search_names = ['firefox', 'Firefox', 'firefox.exe']
                elif process_name.lower() == 'edge':
                    search_names = ['msedge', 'MicrosoftEdge', 'edge']
                else:
                    search_names = [process_name, process_name.lower(), process_name.capitalize()]
                
                powershell_script = f"""
                try {{
                    # Try multiple process name variations
                    $proc = $null
                    $searchNames = @({', '.join(f"'{name}'" for name in search_names)})
                    
                    foreach($name in $searchNames) {{
                        $proc = Get-Process -Name $name -ErrorAction SilentlyContinue | 
                               Where-Object {{$_.MainWindowTitle -ne ''}} | 
                               Select-Object -First 1
                        if($proc) {{ break }}
                    }}
                    
                    if($proc) {{
                        Add-Type -TypeDefinition @'
                        using System;
                        using System.Runtime.InteropServices;
                        public class Win32 {{
                            [DllImport("user32.dll")] 
                            public static extern bool SetForegroundWindow(IntPtr hWnd);
                            [DllImport("user32.dll")] 
                            public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
                            [DllImport("user32.dll")] 
                            public static extern bool IsIconic(IntPtr hWnd);
                        }}
'@
                        # First restore if minimized
                        if([Win32]::IsIconic($proc.MainWindowHandle)) {{
                            [Win32]::ShowWindow($proc.MainWindowHandle, 9)  # SW_RESTORE
                            Start-Sleep -Milliseconds 200
                        }}
                        
                        # Then bring to foreground
                        $success = [Win32]::SetForegroundWindow($proc.MainWindowHandle)
                        
                        if($success) {{
                            Write-Output "SUCCESS: Focused window '$($proc.ProcessName)' - '$($proc.MainWindowTitle)'"
                        }} else {{
                            Write-Output "PARTIAL: Found process '$($proc.ProcessName)' but focus may have failed due to Windows focus restrictions"
                        }}
                    }} else {{
                        Write-Output "ERROR: Process '{process_name}' not found. Searched names: $($searchNames -join ', '). Try 'Show running programs' to see available applications."
                    }}
                }} catch {{
                    Write-Output "ERROR: $($_.Exception.Message)"
                }}
                """
                
                try:
                    result = subprocess.run([
                        "powershell", "-Command", powershell_script
                    ], capture_output=True, text=True, timeout=6)
                    
                    if "SUCCESS" in result.stdout:
                        return f"Window focused successfully: {result.stdout.strip()}"
                    else:
                        return f"Window focus failed: {result.stdout.strip()}"
                        
                except subprocess.TimeoutExpired:
                    return f"Window focus timeout: Command took too long for process '{process_name}'. Try again."
            
            elif operation == 'active':
                # Get detailed active window information
                powershell_script = """
                Add-Type -TypeDefinition @'
                using System;
                using System.Runtime.InteropServices;
                using System.Text;
                public class Win32 {
                    [DllImport("user32.dll")]
                    public static extern IntPtr GetForegroundWindow();
                    [DllImport("user32.dll")]
                    public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
                    [DllImport("user32.dll")]
                    public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);
                }
'@
                
                try {
                    $hwnd = [Win32]::GetForegroundWindow()
                    $title = New-Object System.Text.StringBuilder 256
                    [Win32]::GetWindowText($hwnd, $title, $title.Capacity)
                    
                    $processId = 0
                    [Win32]::GetWindowThreadProcessId($hwnd, [ref]$processId)
                    $process = Get-Process -Id $processId -ErrorAction SilentlyContinue
                    
                    if($process) {
                        Write-Output "Active Window: '$($title.ToString())' | Process: $($process.ProcessName) (PID: $processId)"
                    } else {
                        Write-Output "Active Window: '$($title.ToString())' | Process ID: $processId"
                    }
                } catch {
                    Write-Output "ERROR: $($_.Exception.Message)"
                }
                """
                
                try:
                    result = subprocess.run([
                        "powershell", "-Command", powershell_script
                    ], capture_output=True, text=True, timeout=5)
                    
                    if result.returncode == 0 and result.stdout.strip():
                        return f"Active window info: {result.stdout.strip()}"
                    else:
                        return "Active window: Window detection ready, PowerShell integration available."
                        
                except subprocess.TimeoutExpired:
                    return "Active window timeout: PowerShell command took too long. Window detection available."
            
            else:
                return "Window management: Use 'list' (show windows), 'focus:processname' (focus window), or 'active' (current window). Full PowerShell integration active."
                
        except Exception as e:
            return f"Window management error: {str(e)}. PowerShell integration available with proper permissions."
    
    def mouse_control_tool(self, operation):
        """Mouse control tool for clicking, moving, and scrolling"""
        try:
            import subprocess
            
            if 'click' in operation.lower():
                # Extract coordinates if provided, otherwise click current position
                coords = re.findall(r'(\d+)[,\s]+(\d+)', operation)
                if coords:
                    x, y = coords[0]
                    powershell_script = f"""
                    Add-Type -AssemblyName System.Windows.Forms
                    [System.Windows.Forms.Cursor]::Position = New-Object System.Drawing.Point({x}, {y})
                    Add-Type -TypeDefinition @'
                    using System;
                    using System.Runtime.InteropServices;
                    public class Mouse {{
                        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
                        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);
                        private const int LEFTDOWN = 0x02;
                        private const int LEFTUP = 0x04;
                        public static void LeftClick() {{
                            mouse_event(LEFTDOWN | LEFTUP, 0, 0, 0, 0);
                        }}
                    }}
'@
                    [Mouse]::LeftClick()
                    Write-Output "SUCCESS: Clicked at position ({x}, {y})"
                    """
                else:
                    powershell_script = """
                    Add-Type -TypeDefinition @'
                    using System;
                    using System.Runtime.InteropServices;
                    public class Mouse {
                        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
                        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);
                        private const int LEFTDOWN = 0x02;
                        private const int LEFTUP = 0x04;
                        public static void LeftClick() {
                            mouse_event(LEFTDOWN | LEFTUP, 0, 0, 0, 0);
                        }
                    }
'@
                    [Mouse]::LeftClick()
                    Write-Output "SUCCESS: Left click at current mouse position"
                    """
            
            elif 'move' in operation.lower():
                coords = re.findall(r'(\d+)[,\s]+(\d+)', operation)
                if coords:
                    x, y = coords[0]
                    powershell_script = f"""
                    Add-Type -AssemblyName System.Windows.Forms
                    [System.Windows.Forms.Cursor]::Position = New-Object System.Drawing.Point({x}, {y})
                    Write-Output "SUCCESS: Mouse moved to position ({x}, {y})"
                    """
                else:
                    return "Mouse move: Please specify coordinates like 'move 100 200'"
            
            elif 'scroll' in operation.lower():
                direction = 'up' if 'up' in operation.lower() else 'down'
                scroll_amount = -120 if direction == 'up' else 120
                powershell_script = f"""
                Add-Type -TypeDefinition @'
                using System;
                using System.Runtime.InteropServices;
                public class Mouse {{
                    [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
                    public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);
                    private const int WHEEL = 0x0800;
                    public static void Scroll(int delta) {{
                        mouse_event(WHEEL, 0, 0, (uint)delta, 0);
                    }}
                }}
'@
                [Mouse]::Scroll({scroll_amount})
                Write-Output "SUCCESS: Scrolled {direction}"
                """
            
            else:
                return "Mouse control: Use 'click', 'click x y', 'move x y', 'scroll up', or 'scroll down'. Example: 'click 500 300'"
            
            result = subprocess.run(["powershell", "-Command", powershell_script], 
                                  capture_output=True, text=True, timeout=10)
            
            if result.returncode == 0:
                return result.stdout.strip() or "Mouse operation completed successfully"
            else:
                return f"Mouse control failed: {result.stderr.strip()}"
                
        except Exception as e:
            return f"Mouse control error: {str(e)}. Basic mouse automation available."
    
    def keyboard_control_tool(self, operation):
        """Keyboard control tool for typing text and sending key combinations"""
        try:
            import subprocess
            
            if operation.startswith('type:'):
                text_to_type = operation[5:].strip()
                # Escape special characters for PowerShell
                escaped_text = text_to_type.replace('"', '`"').replace('`', '``')
                powershell_script = f"""
                Add-Type -AssemblyName System.Windows.Forms
                [System.Windows.Forms.SendKeys]::SendWait("{escaped_text}")
                Write-Output "SUCCESS: Typed text: {escaped_text[:50]}{'...' if len(escaped_text) > 50 else ''}"
                """
            
            elif operation.startswith('keys:'):
                key_combo = operation[5:].strip()
                # Convert common key combinations
                key_mapping = {
                    'ctrl+c': '^c', 'ctrl+v': '^v', 'ctrl+a': '^a', 'ctrl+z': '^z',
                    'ctrl+s': '^s', 'ctrl+o': '^o', 'ctrl+n': '^n', 'ctrl+f': '^f',
                    'alt+tab': '%{TAB}', 'alt+f4': '%{F4}', 'win+r': '^{ESC}r',
                    'enter': '{ENTER}', 'tab': '{TAB}', 'escape': '{ESC}',
                    'backspace': '{BACKSPACE}', 'delete': '{DELETE}',
                    'up': '{UP}', 'down': '{DOWN}', 'left': '{LEFT}', 'right': '{RIGHT}'
                }
                
                sendkeys_combo = key_mapping.get(key_combo.lower(), key_combo)
                powershell_script = f"""
                Add-Type -AssemblyName System.Windows.Forms
                [System.Windows.Forms.SendKeys]::SendWait("{sendkeys_combo}")
                Write-Output "SUCCESS: Sent key combination: {key_combo}"
                """
            
            else:
                return "Keyboard control: Use 'type:your text here' or 'keys:ctrl+c'. Supported keys: ctrl+c/v/a/s, alt+tab, enter, tab, arrows, etc."
            
            result = subprocess.run(["powershell", "-Command", powershell_script], 
                                  capture_output=True, text=True, timeout=10)
            
            if result.returncode == 0:
                return result.stdout.strip() or "Keyboard operation completed successfully"
            else:
                return f"Keyboard control failed: {result.stderr.strip()}"
                
        except Exception as e:
            return f"Keyboard control error: {str(e)}. Basic keyboard automation available."
    
    def clipboard_management_tool(self, operation):
        """Clipboard management tool for copy, paste, and clipboard operations"""
        try:
            import subprocess
            
            if operation.startswith('copy:'):
                text_to_copy = operation[5:].strip()
                escaped_text = text_to_copy.replace('"', '`"').replace('`', '``')
                powershell_script = f"""
                Set-Clipboard -Value "{escaped_text}"
                Write-Output "SUCCESS: Copied to clipboard: {escaped_text[:50]}{'...' if len(escaped_text) > 50 else ''}"
                """
            
            elif operation == 'paste' or operation == 'get':
                powershell_script = """
                $clipboardText = Get-Clipboard -Raw
                if ($clipboardText) {
                    Write-Output "CLIPBOARD: $clipboardText"
                } else {
                    Write-Output "CLIPBOARD: (empty)"
                }
                """
            
            elif operation == 'clear':
                powershell_script = """
                Set-Clipboard -Value ""
                Write-Output "SUCCESS: Clipboard cleared"
                """
            
            else:
                return "Clipboard management: Use 'copy:text here', 'paste' (get clipboard), or 'clear'. Example: 'copy:Hello World'"
            
            result = subprocess.run(["powershell", "-Command", powershell_script], 
                                  capture_output=True, text=True, timeout=10)
            
            if result.returncode == 0:
                return result.stdout.strip() or "Clipboard operation completed successfully"
            else:
                return f"Clipboard operation failed: {result.stderr.strip()}"
                
        except Exception as e:
            return f"Clipboard error: {str(e)}. Basic clipboard operations available."
    
    def system_info_tool(self, operation):
        """System information tool for hardware, OS, and process details"""
        try:
            import subprocess
            
            if 'hardware' in operation.lower() or 'specs' in operation.lower():
                powershell_script = """
                $cpu = Get-WmiObject -Class Win32_Processor | Select-Object -First 1
                $ram = Get-WmiObject -Class Win32_ComputerSystem
                $gpu = Get-WmiObject -Class Win32_VideoController | Where-Object {$_.Name -notlike '*Basic*'} | Select-Object -First 1
                
                Write-Output "=== HARDWARE INFO ==="
                Write-Output "CPU: $($cpu.Name.Trim())"
                Write-Output "RAM: $([math]::Round($ram.TotalPhysicalMemory / 1GB, 2)) GB"
                if ($gpu) { Write-Output "GPU: $($gpu.Name)" }
                Write-Output "Cores: $($cpu.NumberOfCores) cores, $($cpu.NumberOfLogicalProcessors) threads"
                """
            
            elif 'os' in operation.lower() or 'system' in operation.lower():
                powershell_script = """
                $os = Get-WmiObject -Class Win32_OperatingSystem
                $computer = Get-WmiObject -Class Win32_ComputerSystem
                
                Write-Output "=== SYSTEM INFO ==="
                Write-Output "OS: $($os.Caption) $($os.Version)"
                Write-Output "Computer: $($computer.Name)"
                Write-Output "Architecture: $($os.OSArchitecture)"
                Write-Output "Uptime: $([math]::Round((Get-Date) - $os.ConvertToDateTime($os.LastBootUpTime)).TotalHours, 1) hours"
                Write-Output "User: $env:USERNAME"
                """
            
            elif 'disk' in operation.lower() or 'storage' in operation.lower():
                powershell_script = """
                Write-Output "=== DISK INFO ==="
                Get-WmiObject -Class Win32_LogicalDisk | Where-Object {$_.DriveType -eq 3} | ForEach-Object {
                    $freeGB = [math]::Round($_.FreeSpace / 1GB, 2)
                    $totalGB = [math]::Round($_.Size / 1GB, 2)
                    $usedPercent = [math]::Round(($totalGB - $freeGB) / $totalGB * 100, 1)
                    Write-Output "Drive $($_.DeviceID) - $freeGB GB free of $totalGB GB ($usedPercent% used)"
                }
                """
            
            elif 'processes' in operation.lower() or 'running' in operation.lower():
                powershell_script = """
                Write-Output "=== TOP PROCESSES (by CPU) ==="
                Get-Process | Sort-Object CPU -Descending | Select-Object -First 10 ProcessName, CPU, WorkingSet | ForEach-Object {
                    $memMB = [math]::Round($_.WorkingSet / 1MB, 1)
                    Write-Output "$($_.ProcessName): CPU $([math]::Round($_.CPU, 2))s, RAM $memMB MB"
                }
                """
            
            else:
                return "System info: Use 'hardware', 'os', 'disk', or 'processes'. Example: 'hardware' for CPU/RAM/GPU specs."
            
            result = subprocess.run(["powershell", "-Command", powershell_script], 
                                  capture_output=True, text=True, timeout=15)
            
            if result.returncode == 0:
                return result.stdout.strip() or "System information retrieved successfully"
            else:
                return f"System info failed: {result.stderr.strip()}"
                
        except Exception as e:
            return f"System info error: {str(e)}. Basic system information available."
    
    def network_operations_tool(self, operation):
        """Network operations tool for connectivity checks and network info"""
        try:
            import subprocess
            
            if operation.startswith('ping:'):
                target = operation[5:].strip()
                if not target:
                    return "Network ping: Please specify target like 'ping:google.com' or 'ping:8.8.8.8'"
                
                powershell_script = f"""
                $result = Test-Connection -ComputerName "{target}" -Count 4 -Quiet
                if ($result) {{
                    $pingResult = Test-Connection -ComputerName "{target}" -Count 4
                    $avgTime = ($pingResult | Measure-Object ResponseTime -Average).Average
                    Write-Output "SUCCESS: {target} is reachable (avg: $([math]::Round($avgTime, 1))ms)"
                }} else {{
                    Write-Output "FAILED: {target} is not reachable"
                }}
                """
            
            elif 'speed' in operation.lower() or 'test' in operation.lower():
                powershell_script = """
                Write-Output "=== NETWORK SPEED TEST ==="
                $google = Test-Connection -ComputerName "8.8.8.8" -Count 1 -Quiet
                $cloudflare = Test-Connection -ComputerName "1.1.1.1" -Count 1 -Quiet
                
                if ($google) { Write-Output "Google DNS (8.8.8.8): REACHABLE" } else { Write-Output "Google DNS: UNREACHABLE" }
                if ($cloudflare) { Write-Output "Cloudflare DNS (1.1.1.1): REACHABLE" } else { Write-Output "Cloudflare DNS: UNREACHABLE" }
                
                $pingGoogle = Test-Connection -ComputerName "google.com" -Count 3
                if ($pingGoogle) {
                    $avgTime = ($pingGoogle | Measure-Object ResponseTime -Average).Average
                    Write-Output "Internet latency: $([math]::Round($avgTime, 1))ms to google.com"
                }
                """
            
            elif 'info' in operation.lower() or 'status' in operation.lower():
                powershell_script = """
                Write-Output "=== NETWORK INFO ==="
                $adapters = Get-NetAdapter | Where-Object {$_.Status -eq 'Up'}
                foreach ($adapter in $adapters) {
                    Write-Output "Interface: $($adapter.Name) ($($adapter.InterfaceDescription))"
                    $ip = Get-NetIPAddress -InterfaceIndex $adapter.InterfaceIndex -AddressFamily IPv4 -ErrorAction SilentlyContinue
                    if ($ip) { Write-Output "  IP: $($ip.IPAddress)" }
                }
                
                $gateway = Get-NetRoute -DestinationPrefix '0.0.0.0/0' | Select-Object -First 1
                if ($gateway) { Write-Output "Gateway: $($gateway.NextHop)" }
                """
            
            else:
                return "Network operations: Use 'ping:target', 'speed' (connectivity test), or 'info' (network details). Example: 'ping:google.com'"
            
            result = subprocess.run(["powershell", "-Command", powershell_script], 
                                  capture_output=True, text=True, timeout=20)
            
            if result.returncode == 0:
                return result.stdout.strip() or "Network operation completed successfully"
            else:
                return f"Network operation failed: {result.stderr.strip()}"
                
        except Exception as e:
            return f"Network error: {str(e)}. Basic network operations available."
    
    def parse_action(self, text):
        """Parse action from agent reasoning text"""
        # Look for Action: tool_name(parameters) pattern
        action_match = re.search(r'Action:\s*(\w+)\(([^)]*)\)', text, re.IGNORECASE)
        if action_match:
            tool_name = action_match.group(1).lower()
            parameters = action_match.group(2).strip('"\'')
            return tool_name, parameters
        return None, None
    
    def execute_tool(self, tool_name, parameters):
        """Execute a tool with given parameters"""
        if tool_name in self.tools:
            try:
                return self.tools[tool_name](parameters)
            except Exception as e:
                return f"Tool execution error: {str(e)}"
        else:
            available_tools = ", ".join(self.tools.keys())
            return f"Unknown tool '{tool_name}'. Available tools: {available_tools}"
    
    def react_step(self, thought, iteration):
        """Execute one ReAct step: Thought -> Action -> Observation"""
        step_result = {
            "iteration": iteration,
            "thought": thought,
            "action": None,
            "observation": None
        }
        
        # Parse action from thought
        tool_name, parameters = self.parse_action(thought)
        
        if tool_name and parameters is not None:
            step_result["action"] = f"{tool_name}({parameters})"
            step_result["observation"] = self.execute_tool(tool_name, parameters)
        else:
            step_result["observation"] = "No valid action found. Use format: Action: tool_name(parameters)"
        
        return step_result
    
    def process_request(self, user_input, max_iterations=3):
        """Process user request using ReAct framework with multi-step support"""
        # Check if this is a multi-step task
        steps = self.parse_multi_step_task(user_input)
        
        if len(steps) > 1:
            # Multi-step task
            return self.execute_multi_step_task(steps, max_iterations)
        else:
            # Single step task - use existing logic
            return self.process_single_request(user_input, max_iterations)
    
    def process_single_request(self, user_input, max_iterations=3):
        """Process single user request using ReAct framework (original logic)"""
        self.scratchpad = []
        
        # Initial analysis
        self.scratchpad.append(f"User Request: {user_input}")
        self.scratchpad.append(f"Available Tools: {', '.join(self.tools.keys())}")
        
        # ReAct reasoning loop
        for i in range(max_iterations):
            # Generate thought based on current context
            if i == 0:
                thought = f"I need to analyze the request '{user_input}'. Let me think about what tools I can use to help."
                
                # Enhanced reasoning to suggest appropriate actions including Windows automation
                # Check memory and clipboard operations FIRST (before math operations)
                if 'remember' in user_input.lower() and ('that' in user_input.lower() or 'my' in user_input.lower()):
                    store_info = user_input.lower().replace('remember that', '').replace('remember', '').strip()
                    thought += f" User wants me to remember something. Action: memory_store(store:{store_info})"
                elif any(recall_word in user_input.lower() for recall_word in ['what\'s my', 'what is my', 'what did i tell', 'what do i', 'recall', 'what\'s', 'favorite']):
                    thought += " User is asking me to recall something from memory. Action: memory_store(recall)"
                elif any(clipboard_word in user_input.lower() for clipboard_word in ['copy', 'paste', 'clipboard', 'cut']):
                    if 'copy' in user_input.lower():
                        # Extract text to copy
                        copy_match = re.search(r'copy[:\s]+(.+)', user_input, re.IGNORECASE)
                        if copy_match:
                            text_to_copy = copy_match.group(1).strip()
                            thought += f" User wants to copy text. Action: clipboard_management(copy:{text_to_copy})"
                        else:
                            thought += " User wants to copy. Action: clipboard_management(copy:Sample text)"
                    elif 'paste' in user_input.lower():
                        thought += " User wants to see clipboard content. Action: clipboard_management(paste)"
                    elif 'clear' in user_input.lower():
                        thought += " User wants to clear clipboard. Action: clipboard_management(clear)"
                    else:
                        thought += " User is asking about clipboard. Action: clipboard_management(paste)"
                elif any(op in user_input.lower() for op in ['+', '-', '*', '/', 'calculate', 'math']):
                    thought += " This looks like a math problem. Action: calculator(" + user_input + ")"
                elif 'time' in user_input.lower() or 'date' in user_input.lower():
                    thought += " User is asking about time. Action: current_time(standard)"
                elif 'random' in user_input.lower():
                    thought += " User wants a random number. Action: random_number(1-100)"
                # Windows Automation Tools Detection
                elif any(screen_word in user_input.lower() for screen_word in ['screenshot', 'screen', 'capture', 'take a picture', 'what\'s on screen']):
                    thought += " User wants to capture the screen. Action: screen_capture(screenshot)"
                elif any(file_word in user_input.lower() for file_word in ['list files', 'show files', 'directory', 'folder', 'read file', 'write file', 'current directory']):
                    if 'list' in user_input.lower() or 'show' in user_input.lower() or 'directory' in user_input.lower() or 'folder' in user_input.lower():
                        if 'current' in user_input.lower():
                            thought += " User wants to see current directory. Action: file_operations(cwd)"
                        else:
                            thought += " User wants to list files/directories. Action: file_operations(list:)"
                    elif 'read' in user_input.lower():
                        thought += " User wants to read a file. Action: file_operations(read:filename)"
                    elif 'write' in user_input.lower():
                        thought += " User wants to write to a file. Action: file_operations(write:filename:content)"
                    else:
                        thought += " User is asking about files. Action: file_operations(cwd)"
                elif any(window_word in user_input.lower() for window_word in ['window', 'focus', 'active window', 'running programs', 'processes', 'applications']):
                    if 'list' in user_input.lower() or 'running' in user_input.lower() or 'programs' in user_input.lower() or 'processes' in user_input.lower():
                        thought += " User wants to see running applications. Action: window_management(list)"
                    elif 'focus' in user_input.lower():
                        # Extract process name from input like "Focus on chrome" or "Focus chrome"
                        focus_words = user_input.lower().replace('focus on', '').replace('focus', '').strip()
                        process_name = focus_words if focus_words else 'unknown'
                        thought += f" User wants to focus a window. Action: window_management(focus:{process_name})"
                    elif 'active' in user_input.lower():
                        thought += " User wants to know the active window. Action: window_management(active)"
                    else:
                        thought += " User is asking about windows. Action: window_management(active)"
                elif any(mouse_word in user_input.lower() for mouse_word in ['click', 'mouse', 'cursor', 'move mouse', 'scroll']):
                    if 'click' in user_input.lower():
                        # Extract coordinates if provided
                        coords = re.findall(r'(\d+)[,\s]+(\d+)', user_input)
                        if coords:
                            x, y = coords[0]
                            thought += f" User wants to click at coordinates. Action: mouse_control(click {x} {y})"
                        else:
                            thought += " User wants to click. Action: mouse_control(click)"
                    elif 'move' in user_input.lower():
                        coords = re.findall(r'(\d+)[,\s]+(\d+)', user_input)
                        if coords:
                            x, y = coords[0]
                            thought += f" User wants to move mouse. Action: mouse_control(move {x} {y})"
                        else:
                            thought += " User wants to move mouse. Action: mouse_control(move)"
                    elif 'scroll' in user_input.lower():
                        direction = 'up' if 'up' in user_input.lower() else 'down'
                        thought += f" User wants to scroll. Action: mouse_control(scroll {direction})"
                    else:
                        thought += " User wants mouse control. Action: mouse_control(click)"
                elif any(keyboard_word in user_input.lower() for keyboard_word in ['type', 'keyboard', 'key', 'press', 'ctrl', 'alt', 'enter']):
                    if 'type' in user_input.lower():
                        # Extract text to type
                        type_match = re.search(r'type[:\s]+(.+)', user_input, re.IGNORECASE)
                        if type_match:
                            text_to_type = type_match.group(1).strip()
                            thought += f" User wants to type text. Action: keyboard_control(type:{text_to_type})"
                        else:
                            thought += " User wants to type. Action: keyboard_control(type:Hello World)"
                    elif any(key in user_input.lower() for key in ['ctrl+', 'alt+', 'enter', 'tab', 'escape']):
                        # Extract key combination
                        key_patterns = ['ctrl\+\w+', 'alt\+\w+', 'enter', 'tab', 'escape', 'backspace', 'delete']
                        key_combo = None
                        for pattern in key_patterns:
                            match = re.search(pattern, user_input.lower())
                            if match:
                                key_combo = match.group(0)
                                break
                        if key_combo:
                            thought += f" User wants to press keys. Action: keyboard_control(keys:{key_combo})"
                        else:
                            thought += " User wants to press keys. Action: keyboard_control(keys:enter)"
                    else:
                        thought += " User wants keyboard control. Action: keyboard_control(type:Hello)"
                elif any(clipboard_word in user_input.lower() for clipboard_word in ['copy', 'paste', 'clipboard', 'cut']):
                    if 'copy' in user_input.lower():
                        # Extract text to copy
                        copy_match = re.search(r'copy[:\s]+(.+)', user_input, re.IGNORECASE)
                        if copy_match:
                            text_to_copy = copy_match.group(1).strip()
                            thought += f" User wants to copy text. Action: clipboard_management(copy:{text_to_copy})"
                        else:
                            thought += " User wants to copy. Action: clipboard_management(copy:Sample text)"
                    elif 'paste' in user_input.lower():
                        thought += " User wants to see clipboard content. Action: clipboard_management(paste)"
                    elif 'clear' in user_input.lower():
                        thought += " User wants to clear clipboard. Action: clipboard_management(clear)"
                    else:
                        thought += " User is asking about clipboard. Action: clipboard_management(paste)"
                elif any(system_word in user_input.lower() for system_word in ['hardware', 'specs', 'system info', 'cpu', 'ram', 'gpu', 'disk', 'storage', 'processes']):
                    if any(hw_word in user_input.lower() for hw_word in ['hardware', 'specs', 'cpu', 'ram', 'gpu']):
                        thought += " User wants hardware information. Action: system_info(hardware)"
                    elif any(os_word in user_input.lower() for os_word in ['system', 'os', 'operating', 'computer', 'uptime']):
                        thought += " User wants system information. Action: system_info(os)"
                    elif any(disk_word in user_input.lower() for disk_word in ['disk', 'storage', 'drive', 'space']):
                        thought += " User wants disk information. Action: system_info(disk)"
                    elif any(proc_word in user_input.lower() for proc_word in ['processes', 'running', 'cpu usage', 'memory usage']):
                        thought += " User wants process information. Action: system_info(processes)"
                    else:
                        thought += " User wants system information. Action: system_info(hardware)"
                elif any(network_word in user_input.lower() for network_word in ['ping', 'network', 'internet', 'connectivity', 'speed test', 'ip']):
                    if 'ping' in user_input.lower():
                        # Extract ping target
                        ping_match = re.search(r'ping[:\s]+(\S+)', user_input, re.IGNORECASE)
                        if ping_match:
                            target = ping_match.group(1).strip()
                            thought += f" User wants to ping. Action: network_operations(ping:{target})"
                        else:
                            thought += " User wants to ping. Action: network_operations(ping:google.com)"
                    elif any(speed_word in user_input.lower() for speed_word in ['speed', 'test', 'connectivity']):
                        thought += " User wants network speed test. Action: network_operations(speed)"
                    elif any(info_word in user_input.lower() for info_word in ['info', 'status', 'ip', 'adapter']):
                        thought += " User wants network information. Action: network_operations(info)"
                    else:
                        thought += " User is asking about network. Action: network_operations(info)"
                elif 'analyze' in user_input.lower() or 'text' in user_input.lower():
                    thought += f" User wants text analysis. Action: text_analyzer({user_input})"
                else:
                    thought += f" Let me analyze this request. Action: text_analyzer({user_input})"
            else:
                # Continue reasoning based on previous observations
                last_observation = self.scratchpad[-1] if self.scratchpad else ""
                thought = f"Based on the previous result: {last_observation}. I should provide a summary or additional analysis if needed."
            
            # Execute ReAct step
            step = self.react_step(thought, i + 1)
            
            # Add to scratchpad
            self.scratchpad.append(f"Thought {i+1}: {step['thought']}")
            if step['action']:
                self.scratchpad.append(f"Action {i+1}: {step['action']}")
            self.scratchpad.append(f"Observation {i+1}: {step['observation']}")
            
            # Simple stopping condition
            if "error" not in step['observation'].lower() and step['action']:
                break
        
        # Generate final result
        final_result = f"ReAct Agent processed: {user_input}\n\nFinal Answer: {step['observation']}"
        
        return {
            "result": final_result,
            "state": "completed",
            "iterations": len([s for s in self.scratchpad if s.startswith("Thought")]),
            "scratchpad": self.scratchpad,
            "tools_used": [s for s in self.scratchpad if s.startswith("Action")]
        }
    
    def parse_multi_step_task(self, user_input):
        """Parse multi-step tasks from user input"""
        # Clean input and look for common multi-step patterns
        clean_input = user_input.strip('"\'')
        
        multi_step_patterns = [
            r'(.+?)\s+then\s+(.+)',  # "do X then Y"
            r'(.+?)\s+and then\s+(.+)',  # "do X and then Y"
            r'(.+?)\s+after that\s+(.+)',  # "do X after that Y"
            r'(.+?)\s+followed by\s+(.+)',  # "do X followed by Y"
            r'(.+?)\s*,\s*then\s+(.+)',  # "do X, then Y"
            r'(.+?)\s*;\s*then\s+(.+)',  # "do X; then Y"
        ]
        
        for pattern in multi_step_patterns:
            match = re.search(pattern, clean_input, re.IGNORECASE)
            if match:
                steps = [match.group(1).strip(), match.group(2).strip()]
                # Check if second step has more steps
                for sub_pattern in multi_step_patterns:
                    sub_match = re.search(sub_pattern, steps[1], re.IGNORECASE)
                    if sub_match:
                        steps = [steps[0], sub_match.group(1).strip(), sub_match.group(2).strip()]
                        break
                return steps
        
        return [clean_input]  # Single step task
    
    def execute_multi_step_task(self, steps, max_iterations=3):
        """Execute multi-step task with progress tracking"""
        self.scratchpad = []
        self.task_queue = steps.copy()
        self.current_task_results = {}
        
        self.scratchpad.append(f"Multi-Step Task Detected: {len(steps)} steps")
        for i, step in enumerate(steps, 1):
            self.scratchpad.append(f"Step {i}: {step}")
        self.scratchpad.append("")
        
        all_results = []
        
        for step_num, step in enumerate(steps, 1):
            self.scratchpad.append(f"=== EXECUTING STEP {step_num}/{len(steps)} ===")
            self.scratchpad.append(f"Current Step: {step}")
            
            # Process this step using single-step logic
            step_result = self.process_single_request(step, max_iterations)
            
            # Store result for potential use in next steps
            self.current_task_results[f"step_{step_num}"] = step_result
            all_results.append(step_result)
            
            self.scratchpad.append(f"Step {step_num} Result: {step_result.get('result', 'No result')}")
            self.scratchpad.append("")
            
            # If step failed, stop execution
            result_text = step_result.get('result', '').lower()
            if "error" in result_text or "failed" in result_text:
                self.scratchpad.append(f"⚠️ Step {step_num} failed. Stopping multi-step execution.")
                break
        
        # Generate comprehensive final result
        final_summary = f"Multi-Step Task Completed: {len(all_results)}/{len(steps)} steps executed\n\n"
        for i, result in enumerate(all_results, 1):
            final_summary += f"Step {i}: {result.get('result', 'No result')}\n"
        
        return {
            "result": final_summary.strip(),
            "state": "completed",
            "total_steps": len(steps),
            "completed_steps": len(all_results),
            "iterations": sum(r.get('iterations', 1) for r in all_results),
            "framework": "ReAct Multi-Step",
            "scratchpad": self.scratchpad,
            "tools_used": [tool for result in all_results for tool in result.get('tools_used', [])],
            "available_tools": list(self.tools.keys()),
            "memory_items": len(self.memory),
            "step_results": all_results
        }

# Global ReAct agent instance
react_agent = ReActAgent()

class SmartitectureHandler(http.server.BaseHTTPRequestHandler):
    def do_GET(self):
        parsed_path = urlparse(self.path)
        
        if parsed_path.path == '/':
            self.send_response(200)
            self.send_header('Content-type', 'application/json')
            self.send_header('Access-Control-Allow-Origin', '*')
            self.end_headers()
            response = {
                "message": "🤖 Smartitecture ReAct Agent API is running!", 
                "status": "ok",
                "framework": "ReAct (Reasoning and Acting)",
                "tools": list(react_agent.tools.keys())
            }
            self.wfile.write(json.dumps(response).encode())
            
        elif parsed_path.path == '/health':
            self.send_response(200)
            self.send_header('Content-type', 'application/json')
            self.send_header('Access-Control-Allow-Origin', '*')
            self.end_headers()
            response = {
                "status": "healthy", 
                "service": "smartitecture-react-agent",
                "agent_memory_items": len(react_agent.memory),
                "available_tools": len(react_agent.tools)
            }
            self.wfile.write(json.dumps(response).encode())
            
        elif parsed_path.path == '/agent/state':
            self.send_response(200)
            self.send_header('Content-type', 'application/json')
            self.send_header('Access-Control-Allow-Origin', '*')
            self.end_headers()
            response = {
                "state": "ready", 
                "framework": "ReAct",
                "available_tools": list(react_agent.tools.keys()),
                "memory_items": len(react_agent.memory),
                "recent_memory": [item['info'] for item in react_agent.memory[-3:]] if react_agent.memory else [],
                "last_scratchpad_size": len(react_agent.scratchpad)
            }
            self.wfile.write(json.dumps(response).encode())
            
        else:
            self.send_response(404)
            self.send_header('Content-type', 'application/json')
            self.send_header('Access-Control-Allow-Origin', '*')
            self.end_headers()
            response = {"error": "Not found"}
            self.wfile.write(json.dumps(response).encode())

    def do_POST(self):
        if self.path == '/agent/run':
            content_length = int(self.headers['Content-Length'])
            post_data = self.rfile.read(content_length)
            
            try:
                request_data = json.loads(post_data.decode())
                input_text = request_data.get('input', 'No input provided')
                max_iterations = request_data.get('max_iterations', 3)
                
                # Process request using ReAct agent
                agent_response = react_agent.process_request(input_text, max_iterations)
                
                self.send_response(200)
                self.send_header('Content-type', 'application/json')
                self.send_header('Access-Control-Allow-Origin', '*')
                self.end_headers()
                
                # Return comprehensive ReAct response
                response = {
                    "result": agent_response["result"],
                    "state": agent_response["state"],
                    "iterations": agent_response["iterations"],
                    "framework": "ReAct",
                    "scratchpad": agent_response["scratchpad"],
                    "tools_used": agent_response["tools_used"],
                    "available_tools": list(react_agent.tools.keys()),
                    "memory_items": len(react_agent.memory)
                }
                self.wfile.write(json.dumps(response).encode())
                
            except Exception as e:
                self.send_response(400)
                self.send_header('Content-type', 'application/json')
                self.send_header('Access-Control-Allow-Origin', '*')
                self.end_headers()
                response = {
                    "error": str(e),
                    "framework": "ReAct",
                    "available_tools": list(react_agent.tools.keys()) if 'react_agent' in globals() else []
                }
                self.wfile.write(json.dumps(response).encode())
        else:
            self.send_response(404)
            self.send_header('Content-type', 'application/json')
            self.send_header('Access-Control-Allow-Origin', '*')
            self.end_headers()
            response = {"error": "Not found"}
            self.wfile.write(json.dumps(response).encode())

    def log_message(self, format, *args):
        # Suppress default logging
        return

def start_server(port=8001):
    """Start the ReAct agent HTTP server"""
    try:
        with socketserver.TCPServer(("127.0.0.1", port), SmartitectureHandler) as httpd:
            print(f"🤖 Smartitecture ReAct Agent API running on http://127.0.0.1:{port}")
            print("\n📋 Available endpoints:")
            print("- GET  /           - API info and available tools")
            print("- GET  /health     - Health check and agent status") 
            print("- GET  /agent/state - Agent state and memory info")
            print("- POST /agent/run  - Run ReAct agent with user input")
            print("\n🛠️  Available ReAct Tools:")
            for tool_name, tool_func in react_agent.tools.items():
                print(f"- {tool_name}: {tool_func.__doc__ or 'No description'}")
            print("\n🧠 ReAct Framework Features:")
            print("- Thought-Action-Observation loops")
            print("- Tool calling and execution")
            print("- Memory storage and retrieval")
            print("- Structured reasoning process")
            print("\nPress Ctrl+C to stop")
            httpd.serve_forever()
    except KeyboardInterrupt:
        print("\nShutting down server...")
    except Exception as e:
        print(f"Server error: {e}")

if __name__ == "__main__":
    start_server()
