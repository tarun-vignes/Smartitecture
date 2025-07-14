#!/usr/bin/env python3
"""
Startup script for Smartitecture Python API
This script starts the FastAPI server that the C# application will communicate with
"""

import sys
import os
import uvicorn
from pathlib import Path

# Add the project root to Python path
project_root = Path(__file__).parent
sys.path.insert(0, str(project_root))

def main():
    """Start the FastAPI server"""
    print("🚀 Starting Smartitecture Python API...")
    print(f"📁 Project root: {project_root}")
    print(f"🌐 Server will be available at: http://127.0.0.1:8000")
    print(f"📖 API documentation: http://127.0.0.1:8000/docs")
    print("=" * 50)
    
    try:
        # Start the FastAPI server
        uvicorn.run(
            "smartitecture.api.main:app",
            host="127.0.0.1",
            port=8000,
            reload=True,  # Enable auto-reload for development
            log_level="info"
        )
    except KeyboardInterrupt:
        print("\n🛑 Server stopped by user")
    except Exception as e:
        print(f"❌ Error starting server: {e}")
        sys.exit(1)

if __name__ == "__main__":
    main()
