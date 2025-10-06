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
            'window_management': self.window_management_tool
        }
        self.memory = []
        self.scratchpad = []
    
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
