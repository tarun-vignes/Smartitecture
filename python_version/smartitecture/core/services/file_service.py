import os
import shutil
from pathlib import Path
from typing import Optional, List
from .base_service import BaseService

class FileService(BaseService):
    def __init__(self):
        super().__init__()
        self.base_dir = Path.home() / ".smartitecture"
        
    async def initialize(self) -> None:
        await super().initialize()
        if not self.base_dir.exists():
            self.base_dir.mkdir(parents=True, exist_ok=True)
            
    async def create_file(self, path: Path, content: str = "") -> bool:
        """Create a new file"""
        try:
            path.parent.mkdir(parents=True, exist_ok=True)
            with open(path, 'w') as f:
                f.write(content)
            return True
        except Exception as e:
            self.logger.error(f"Error creating file {path}: {str(e)}")
            return False
            
    async def read_file(self, path: Path) -> Optional[str]:
        """Read file contents"""
        try:
            if not path.exists():
                return None
            with open(path, 'r') as f:
                return f.read()
        except Exception as e:
            self.logger.error(f"Error reading file {path}: {str(e)}")
            return None
            
    async def delete_file(self, path: Path) -> bool:
        """Delete a file"""
        try:
            if path.exists():
                os.remove(path)
            return True
        except Exception as e:
            self.logger.error(f"Error deleting file {path}: {str(e)}")
            return False
            
    async def list_files(self, directory: Path) -> List[Path]:
        """List files in directory"""
        try:
            if not directory.exists():
                return []
            return [f for f in directory.iterdir() if f.is_file()]
        except Exception as e:
            self.logger.error(f"Error listing files in {directory}: {str(e)}")
            return []
