#!/usr/bin/env python3
"""
Minimal HTTP server for Smartitecture backend
Uses only Python standard library - no external dependencies
"""

import json
import http.server
import socketserver
from urllib.parse import urlparse, parse_qs
from datetime import datetime
import threading

class SmartitectureHandler(http.server.BaseHTTPRequestHandler):
    def do_GET(self):
        """Handle GET requests"""
        parsed_path = urlparse(self.path)
        path = parsed_path.path
        
        if path == '/':
            self.send_json_response({"message": "Smartitecture API is running!", "timestamp": datetime.now().isoformat()})
        elif path == '/health':
            self.send_json_response({"status": "healthy", "service": "smartitecture-api"})
        elif path == '/test':
            self.send_json_response({"message": "Test endpoint working!", "status": "ok"})
        elif path == '/agent/state':
            self.send_json_response({
                "state": "idle",
                "iteration": 0,
                "memory": []
            })
        else:
            self.send_error(404, "Endpoint not found")
    
    def do_POST(self):
        """Handle POST requests"""
        parsed_path = urlparse(self.path)
        path = parsed_path.path
        
        if path == '/agent/run':
            # Read the request body
            content_length = int(self.headers.get('Content-Length', 0))
            post_data = self.rfile.read(content_length)
            
            try:
                request_data = json.loads(post_data.decode('utf-8'))
                input_text = request_data.get('input', '')
                max_iterations = request_data.get('max_iterations', 10)
                
                # Simple mock response
                response = {
                    "result": f"Processed: {input_text}",
                    "state": "completed",
                    "iterations": 1
                }
                self.send_json_response(response)
            except json.JSONDecodeError:
                self.send_error(400, "Invalid JSON")
        else:
            self.send_error(404, "Endpoint not found")
    
    def send_json_response(self, data, status_code=200):
        """Send JSON response with proper headers"""
        self.send_response(status_code)
        self.send_header('Content-Type', 'application/json')
        self.send_header('Access-Control-Allow-Origin', '*')
        self.send_header('Access-Control-Allow-Methods', 'GET, POST, OPTIONS')
        self.send_header('Access-Control-Allow-Headers', 'Content-Type')
        self.end_headers()
        
        json_data = json.dumps(data, indent=2)
        self.wfile.write(json_data.encode('utf-8'))
    
    def do_OPTIONS(self):
        """Handle CORS preflight requests"""
        self.send_response(200)
        self.send_header('Access-Control-Allow-Origin', '*')
        self.send_header('Access-Control-Allow-Methods', 'GET, POST, OPTIONS')
        self.send_header('Access-Control-Allow-Headers', 'Content-Type')
        self.end_headers()

def start_server(port=8001):
    """Start the HTTP server"""
    with socketserver.TCPServer(("127.0.0.1", port), SmartitectureHandler) as httpd:
        print(f"Starting Smartitecture API server...")
        print(f"Server running at http://127.0.0.1:{port}/")
        print("Available endpoints:")
        print(f"- http://127.0.0.1:{port}/")
        print(f"- http://127.0.0.1:{port}/health")
        print(f"- http://127.0.0.1:{port}/test")
        print(f"- http://127.0.0.1:{port}/agent/state")
        print(f"- http://127.0.0.1:{port}/agent/run")
        print("\nPress Ctrl+C to stop the server")
        
        try:
            httpd.serve_forever()
        except KeyboardInterrupt:
            print("\nServer stopped.")

if __name__ == "__main__":
    start_server()
