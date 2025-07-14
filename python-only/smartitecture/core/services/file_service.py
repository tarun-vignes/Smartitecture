import os
from pathlib import Path
from typing import Optional
from ..config import Config

class FileService:
    def __init__(self):
        self.data_dir = Config.DATA_DIR
        
    def initialize(self):
        """Initialize file service"""
        # Create data directory if it doesn't exist
        self.data_dir.mkdir(parents=True, exist_ok=True)
        
    def create_file(self, path: Path, content: str = "") -> bool:
        """Create a new file"""
        try:
            path.parent.mkdir(parents=True, exist_ok=True)
            with open(path, 'w') as f:
                f.write(content)
            return True
        except Exception as e:
            print(f"Error creating file: {str(e)}")
            return False
            
    def read_file(self, path: Path) -> Optional[str]:
        """Read file contents"""
        try:
            if not path.exists():
                return None
            with open(path, 'r') as f:
                return f.read()
        except Exception as e:
            print(f"Error reading file: {str(e)}")
            return None
            
    def delete_file(self, path: Path) -> bool:
        """Delete a file"""
        try:
            if path.exists():
                os.remove(path)
            return True
        except Exception as e:
            print(f"Error deleting file: {str(e)}")
            return False
