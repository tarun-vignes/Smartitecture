#!/usr/bin/env python3
"""
Startup script for the Smartitecture API.

This script initializes and starts the FastAPI server with the ReAct agent.
"""
import os
import sys
import uvicorn
import logging
from pathlib import Path

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s',
    handlers=[
        logging.StreamHandler(),
        logging.FileHandler('api.log')
    ]
)
logger = logging.getLogger(__name__)

def main():
    """Main entry point for the application."""
    # Add the project root to the Python path
    project_root = Path(__file__).parent
    sys.path.insert(0, str(project_root))
    
    # Load environment variables
    from dotenv import load_dotenv
    load_dotenv()
    
    # Get configuration from environment variables
    host = os.getenv("HOST", "127.0.0.1")
    port = int(os.getenv("PORT", "8000"))
    reload = os.getenv("RELOAD", "false").lower() == "true"
    log_level = os.getenv("LOG_LEVEL", "info")
    
    logger.info(f"Starting Smartitecture API on {host}:{port}")
    logger.info(f"Environment: {'development' if reload else 'production'}")
    
    # Start the FastAPI server
    uvicorn.run(
        "smartitecture.api.main:app",
        host=host,
        port=port,
        reload=reload,
        log_level=log_level,
        workers=1  # For development, use 1 worker
    )

if __name__ == "__main__":
    main()
