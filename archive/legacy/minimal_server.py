#!/usr/bin/env python3
"""
Smartitecture Advanced AI Agent Framework v2.0
Enhanced ReAct pattern with local LLM integration and advanced automation
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
import subprocess
import os
import sys
import tempfile
import shutil
import psutil
import requests
from datetime import datetime
from pathlib import Path

class AdvancedReActAgent:
    """Advanced ReAct Agent with Local LLM Integration and Enhanced Automation"""
    
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
            'system_monitor': self.system_monitor_tool,
            'process_manager': self.process_manager_tool,
            'network_tools': self.network_tools_tool,
            'workflow_automation': self.workflow_automation_tool,
            'ai_analysis': self.ai_analysis_tool,
            'performance_optimizer': self.performance_optimizer_tool
        }
        self.memory = []
        self.scratchpad = []
        self.ollama_url = "http://localhost:11434"
        self.workflows = []
    
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
    
    def screen_capture_tool(self, params):
        """Advanced screenshot with AI analysis capabilities"""
        try:
            # Use PowerShell to take screenshot
            ps_script = """
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
$bounds = [System.Windows.Forms.Screen]::PrimaryScreen.Bounds
$bitmap = New-Object System.Drawing.Bitmap $bounds.Width, $bounds.Height
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
$graphics.CopyFromScreen($bounds.Location, [System.Drawing.Point]::Empty, $bounds.Size)
$temp_path = [System.IO.Path]::GetTempPath() + "smartitecture_screenshot_" + (Get-Date -Format "yyyyMMdd_HHmmss") + ".png"
$bitmap.Save($temp_path, [System.Drawing.Imaging.ImageFormat]::Png)
$graphics.Dispose()
$bitmap.Dispose()
Write-Output $temp_path
"""
            result = subprocess.run(['powershell', '-Command', ps_script], 
                                  capture_output=True, text=True, shell=True)
            
            if result.returncode == 0:
                screenshot_path = result.stdout.strip()
                file_size = os.path.getsize(screenshot_path) if os.path.exists(screenshot_path) else 0
                
                # Basic analysis without external dependencies
                analysis = self._basic_screenshot_analysis(screenshot_path)
                
                return f"📸 Smart Screenshot Analysis:\n" + \
                       f"• Path: {screenshot_path}\n" + \
                       f"• Size: {file_size:,} bytes\n" + \
                       f"• Resolution: {analysis['resolution']}\n" + \
                       f"• Timestamp: {analysis['timestamp']}\n" + \
                       f"• Status: Screenshot captured successfully"
            else:
                return f"Screenshot failed: {result.stderr}"
        except Exception as e:
            return f"Screenshot error: {str(e)}"
    
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
    
    def window_management_tool(self, params):
        """Advanced window management with smart positioning"""
        try:
            if 'focus' in params.lower():
                process_name = self._extract_process_name(params)
                if not process_name:
                    return "No process name found in command"
                
                ps_script = f"""
Get-Process -Name "{process_name}" -ErrorAction SilentlyContinue | ForEach-Object {{
    $proc = $_
    Add-Type -TypeDefinition 'using System; using System.Runtime.InteropServices; public class Win32 {{ [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd); [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow); }}'
    [Win32]::ShowWindow($proc.MainWindowHandle, 9)
    [Win32]::SetForegroundWindow($proc.MainWindowHandle)
    Write-Output "✅ Focused on $($proc.ProcessName) (PID: $($proc.Id))"
}}
"""
            elif 'arrange' in params.lower() or 'tile' in params.lower():
                # Smart window arrangement
                ps_script = """
Add-Type -TypeDefinition 'using System; using System.Runtime.InteropServices; public class Win32 { [DllImport("user32.dll")] public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags); }'
$processes = Get-Process | Where-Object { $_.MainWindowTitle -ne "" } | Select-Object -First 4
$screenWidth = [System.Windows.Forms.Screen]::PrimaryScreen.Bounds.Width
$screenHeight = [System.Windows.Forms.Screen]::PrimaryScreen.Bounds.Height
$count = 0
foreach ($proc in $processes) {
    $x = ($count % 2) * ($screenWidth / 2)
    $y = [math]::Floor($count / 2) * ($screenHeight / 2)
    [Win32]::SetWindowPos($proc.MainWindowHandle, [IntPtr]::Zero, $x, $y, $screenWidth/2, $screenHeight/2, 0x0040)
    $count++
}
Write-Output "Arranged $count windows in tile layout"
"""
            else:
                # Enhanced window listing with details
                ps_script = """
$windows = Get-Process | Where-Object { $_.MainWindowTitle -ne "" } | Select-Object ProcessName, Id, MainWindowTitle, WorkingSet64
$totalMemory = ($windows | Measure-Object WorkingSet64 -Sum).Sum / 1MB
Write-Output "🪟 Active Windows Analysis:"
Write-Output "Total Windows: $($windows.Count)"
Write-Output "Total Memory Usage: $([math]::Round($totalMemory, 2)) MB"
Write-Output ""
$windows | ForEach-Object { Write-Output "• $($_.ProcessName) - $($_.MainWindowTitle) ($(([math]::Round($_.WorkingSet64/1MB, 1))) MB)" }
"""
            
            result = subprocess.run(['powershell', '-Command', ps_script], 
                                  capture_output=True, text=True, shell=True)
            
            if result.returncode == 0:
                return result.stdout
            else:
                return f"Window operation failed: {result.stderr}"
                return "Window management: Use 'list' (show windows), 'focus:processname' (focus window), or 'active' (current window). Full PowerShell integration active."
                
        except Exception as e:
            return f"Window management error: {str(e)}. PowerShell integration available with proper permissions."
    
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
        """Process user request using ReAct framework"""
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
                if any(op in user_input.lower() for op in ['+', '-', '*', '/', 'calculate', 'math']):
                    thought += " This looks like a math problem. Action: calculator(" + user_input + ")"
                elif 'time' in user_input.lower() or 'date' in user_input.lower():
                    thought += " User is asking about time. Action: current_time(standard)"
                elif 'random' in user_input.lower():
                    thought += " User wants a random number. Action: random_number(1-100)"
                elif 'remember' in user_input.lower() and ('that' in user_input.lower() or 'my' in user_input.lower()):
                    # Extract the information to store
                    store_info = user_input.lower().replace('remember that', '').replace('remember', '').strip()
                    thought += f" User wants me to remember something. Action: memory_store(store:{store_info})"
                elif any(recall_word in user_input.lower() for recall_word in ['what\'s my', 'what is my', 'what did i tell', 'what do i', 'recall', 'what\'s', 'favorite']):
                    thought += " User is asking me to recall something from memory. Action: memory_store(recall)"
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

    def mouse_control_tool(self, params):
        """Advanced mouse control and automation"""
        try:
            if 'move' in params.lower():
                # Move mouse to center or specific coordinates
                ps_script = """
Add-Type -AssemblyName System.Windows.Forms
$screen = [System.Windows.Forms.Screen]::PrimaryScreen.Bounds
$centerX = $screen.Width / 2
$centerY = $screen.Height / 2
[System.Windows.Forms.Cursor]::Position = New-Object System.Drawing.Point($centerX, $centerY)
Write-Output "🖱️ Mouse moved to center: ($centerX, $centerY)"
"""
            elif 'click' in params.lower():
                ps_script = """
Add-Type -TypeDefinition 'using System; using System.Runtime.InteropServices; public class Mouse { [DllImport("user32.dll")] public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, IntPtr dwExtraInfo); }'
[Mouse]::mouse_event(0x02, 0, 0, 0, [IntPtr]::Zero)  # Left button down
[Mouse]::mouse_event(0x04, 0, 0, 0, [IntPtr]::Zero)  # Left button up
Write-Output "🖱️ Mouse clicked at current position"
"""
            else:
                return "🖱️ Mouse Control: Use 'move' to center mouse or 'click' to click"
            
            result = subprocess.run(['powershell', '-Command', ps_script], 
                                  capture_output=True, text=True, shell=True)
            return result.stdout if result.returncode == 0 else f"Mouse control failed: {result.stderr}"
        except Exception as e:
            return f"Mouse control error: {str(e)}"
    
    def keyboard_control_tool(self, params):
        """Advanced keyboard automation and text input"""
        try:
            if params.startswith('type:'):
                text_to_type = params[5:].strip()
                ps_script = f"""
Add-Type -AssemblyName System.Windows.Forms
[System.Windows.Forms.SendKeys]::SendWait("{text_to_type}")
Write-Output "⌨️ Typed: {text_to_type}"
"""
            elif 'enter' in params.lower():
                ps_script = """
Add-Type -AssemblyName System.Windows.Forms
[System.Windows.Forms.SendKeys]::SendWait("{ENTER}")
Write-Output "⌨️ Pressed Enter key"
"""
            elif 'tab' in params.lower():
                ps_script = """
Add-Type -AssemblyName System.Windows.Forms
[System.Windows.Forms.SendKeys]::SendWait("{TAB}")
Write-Output "⌨️ Pressed Tab key"
"""
            else:
                return "⌨️ Keyboard Control: Use 'type:text', 'enter', or 'tab'"
            
            result = subprocess.run(['powershell', '-Command', ps_script], 
                                  capture_output=True, text=True, shell=True)
            return result.stdout if result.returncode == 0 else f"Keyboard control failed: {result.stderr}"
        except Exception as e:
            return f"Keyboard control error: {str(e)}"
    
    def system_monitor_tool(self, params):
        """Advanced system monitoring and performance metrics"""
        try:
            if 'performance' in params.lower() or 'cpu' in params.lower():
                ps_script = """
$cpu = Get-Counter "\Processor(_Total)\% Processor Time" -SampleInterval 1 -MaxSamples 1
$memory = Get-WmiObject -Class Win32_OperatingSystem
$disk = Get-WmiObject -Class Win32_LogicalDisk -Filter "DeviceID='C:'"
$processes = Get-Process | Sort-Object CPU -Descending | Select-Object -First 5

Write-Output "📊 System Performance Monitor:"
Write-Output "• CPU Usage: $([math]::Round(100 - $cpu.CounterSamples[0].CookedValue, 2))%"
Write-Output "• Memory: $([math]::Round(($memory.TotalVisibleMemorySize - $memory.FreePhysicalMemory) / $memory.TotalVisibleMemorySize * 100, 2))% used"
Write-Output "• Disk C: $([math]::Round(($disk.Size - $disk.FreeSpace) / $disk.Size * 100, 2))% used"
Write-Output "• Top Processes by CPU:"
$processes | ForEach-Object { Write-Output "  - $($_.ProcessName): $([math]::Round($_.CPU, 2))s" }
"""
            elif 'network' in params.lower():
                ps_script = """
$adapters = Get-WmiObject -Class Win32_NetworkAdapter | Where-Object {$_.NetConnectionStatus -eq 2}
Write-Output "🌐 Network Status:"
Write-Output "• Active Adapters: $($adapters.Count)"
$adapters | ForEach-Object { Write-Output "  - $($_.Name): Connected" }
"""
            else:
                ps_script = """
Write-Output "📊 System Monitor Options:"
Write-Output "• Use 'performance' for CPU/Memory/Disk stats"
Write-Output "• Use 'network' for network adapter status"
"""
            
            result = subprocess.run(['powershell', '-Command', ps_script], 
                                  capture_output=True, text=True, shell=True)
            return result.stdout if result.returncode == 0 else f"System monitor failed: {result.stderr}"
        except Exception as e:
            return f"System monitor error: {str(e)}"
    
    def process_manager_tool(self, params):
        """Advanced process management and control"""
        try:
            if 'list' in params.lower():
                ps_script = """
$processes = Get-Process | Sort-Object WorkingSet64 -Descending | Select-Object -First 10
Write-Output "🔧 Top 10 Processes by Memory Usage:"
$processes | ForEach-Object { 
    $memMB = [math]::Round($_.WorkingSet64 / 1MB, 1)
    Write-Output "• $($_.ProcessName) (PID: $($_.Id)): $memMB MB"
}
"""
            elif 'kill' in params.lower():
                return "🔧 Process termination requires specific process name for safety"
            else:
                ps_script = """
Write-Output "🔧 Process Manager:"
Write-Output "• Use 'list' to show top processes by memory"
Write-Output "• Process termination available with specific commands"
"""
            
            result = subprocess.run(['powershell', '-Command', ps_script], 
                                  capture_output=True, text=True, shell=True)
            return result.stdout if result.returncode == 0 else f"Process manager failed: {result.stderr}"
        except Exception as e:
            return f"Process manager error: {str(e)}"
    
    def network_tools_tool(self, params):
        """Network diagnostics and connectivity tools"""
        try:
            if 'ping' in params.lower():
                ps_script = """
$ping = Test-Connection -ComputerName google.com -Count 4 -Quiet
if ($ping) {
    $result = Test-Connection -ComputerName google.com -Count 1
    Write-Output "🌐 Network Connectivity: ✅ Online"
    Write-Output "• Ping to google.com: $($result.ResponseTime)ms"
} else {
    Write-Output "🌐 Network Connectivity: ❌ Offline"
}
"""
            elif 'speed' in params.lower():
                return "🌐 Network speed test requires external tools - basic connectivity available"
            else:
                ps_script = """
Write-Output "🌐 Network Tools:"
Write-Output "• Use 'ping' to test internet connectivity"
Write-Output "• Use 'speed' for speed test info"
"""
            
            result = subprocess.run(['powershell', '-Command', ps_script], 
                                  capture_output=True, text=True, shell=True)
            return result.stdout if result.returncode == 0 else f"Network tools failed: {result.stderr}"
        except Exception as e:
            return f"Network tools error: {str(e)}"
    
    def workflow_automation_tool(self, params):
        """Create and execute automated workflows"""
        try:
            if 'create' in params.lower():
                workflow_name = f"workflow_{len(self.workflows) + 1}"
                self.workflows.append({
                    'name': workflow_name,
                    'steps': params.replace('create:', '').strip().split(' then '),
                    'created': datetime.now().isoformat()
                })
                return f"🎯 Created workflow '{workflow_name}' with {len(self.workflows[-1]['steps'])} steps"
            elif 'list' in params.lower():
                if not self.workflows:
                    return "🎯 No workflows created yet. Use 'create:step1 then step2' to create one"
                result = "🎯 Available Workflows:\n"
                for wf in self.workflows:
                    result += f"• {wf['name']}: {len(wf['steps'])} steps\n"
                return result
            else:
                return "🎯 Workflow Automation: Use 'create:step1 then step2' or 'list' to see workflows"
        except Exception as e:
            return f"Workflow automation error: {str(e)}"
    
    def ai_analysis_tool(self, params):
        """AI-powered analysis and insights"""
        try:
            # Check if Ollama is available
            if self._check_ollama_connection():
                return self._ollama_analysis(params)
            else:
                # Fallback to basic analysis
                return f"🔮 Basic AI Analysis: {params}\n" + \
                       f"• Text length: {len(params)} characters\n" + \
                       f"• Word count: {len(params.split())} words\n" + \
                       f"• Complexity: {'High' if len(params) > 100 else 'Medium' if len(params) > 50 else 'Low'}\n" + \
                       f"• Suggestion: Consider using local LLM (Ollama) for advanced analysis"
        except Exception as e:
            return f"AI analysis error: {str(e)}"
    
    def performance_optimizer_tool(self, params):
        """System performance optimization suggestions"""
        try:
            ps_script = """
$cpu = Get-Counter "\Processor(_Total)\% Processor Time" -SampleInterval 1 -MaxSamples 1
$memory = Get-WmiObject -Class Win32_OperatingSystem
$processes = Get-Process | Sort-Object WorkingSet64 -Descending | Select-Object -First 3

Write-Output "⚡ Performance Optimization Analysis:"
$cpuUsage = 100 - $cpu.CounterSamples[0].CookedValue
$memUsage = ($memory.TotalVisibleMemorySize - $memory.FreePhysicalMemory) / $memory.TotalVisibleMemorySize * 100

Write-Output "• CPU Usage: $([math]::Round($cpuUsage, 2))%"
Write-Output "• Memory Usage: $([math]::Round($memUsage, 2))%"

if ($cpuUsage -gt 80) {
    Write-Output "• ⚠️ High CPU usage detected - consider closing unnecessary applications"
}
if ($memUsage -gt 80) {
    Write-Output "• ⚠️ High memory usage detected - top consumers:"
    $processes | ForEach-Object { Write-Output "    - $($_.ProcessName): $([math]::Round($_.WorkingSet64/1MB, 1)) MB" }
}
if ($cpuUsage -lt 50 -and $memUsage -lt 70) {
    Write-Output "• ✅ System performance is optimal"
}
"""
            
            result = subprocess.run(['powershell', '-Command', ps_script], 
                                  capture_output=True, text=True, shell=True)
            return result.stdout if result.returncode == 0 else f"Performance optimizer failed: {result.stderr}"
        except Exception as e:
            return f"Performance optimizer error: {str(e)}"
    
    def _basic_screenshot_analysis(self, screenshot_path):
        """Basic screenshot analysis without external dependencies"""
        try:
            file_stats = os.stat(screenshot_path)
            return {
                'resolution': 'Available via PowerShell',
                'timestamp': datetime.fromtimestamp(file_stats.st_mtime).strftime('%Y-%m-%d %H:%M:%S'),
                'file_size': file_stats.st_size
            }
        except:
            return {
                'resolution': 'Unknown',
                'timestamp': datetime.now().strftime('%Y-%m-%d %H:%M:%S'),
                'file_size': 0
            }
    
    def _check_ollama_connection(self):
        """Check if Ollama is running and accessible"""
        try:
            response = requests.get(f"{self.ollama_url}/api/tags", timeout=2)
            return response.status_code == 200
        except:
            return False
    
    def _ollama_analysis(self, params):
        """Use Ollama for advanced AI analysis"""
        try:
            payload = {
                "model": "llama3.1",
                "prompt": f"Analyze this request and provide insights: {params}",
                "stream": False
            }
            response = requests.post(f"{self.ollama_url}/api/generate", json=payload, timeout=10)
            if response.status_code == 200:
                result = response.json()
                return f"🧠 Ollama AI Analysis:\n{result.get('response', 'No response')}"
            else:
                return "🧠 Ollama connection failed - using basic analysis"
        except Exception as e:
            return f"🧠 Ollama analysis error: {str(e)}"
    
    def _extract_process_name(self, text):
        """Extract process name from user input"""
        # Remove common words and extract the likely process name
        text = text.lower().replace('focus on', '').replace('focus', '').replace('window', '').strip()
        # Handle common application names
        app_mappings = {
            'chrome': 'chrome',
            'firefox': 'firefox',
            'edge': 'msedge',
            'notepad': 'notepad',
            'calculator': 'calc',
            'explorer': 'explorer',
            'code': 'code',
            'visual studio': 'devenv'
        }
        return app_mappings.get(text, text)

# Global ReAct agent instance
react_agent = AdvancedReActAgent()

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
