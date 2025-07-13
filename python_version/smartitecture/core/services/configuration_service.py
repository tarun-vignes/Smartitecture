import json
from typing import Any, TypeVar, Generic
from pathlib import Path
from .base_service import BaseService
from .interfaces import IBaseService

T = TypeVar('T')

class ConfigurationService(BaseService):
    def __init__(self):
        super().__init__()
        self.config_dir = Path.home() / ".smartitecture"
        self.config_file = self.config_dir / "config.json"
        self.config: dict = {}
        
    async def initialize(self) -> None:
        await super().initialize()
        await self.load_config()
        
    async def load_config(self) -> None:
        """Load configuration from file"""
        try:
            if not self.config_dir.exists():
                self.config_dir.mkdir(parents=True, exist_ok=True)
                
            if not self.config_file.exists():
                self.config = {
                    "theme": "Dark",
                    "model": "GPT-4",
                    "api_key": "",
                    "version": "1.0.0"
                }
                await self.save_config()
            else:
                with open(self.config_file, 'r') as f:
                    self.config = json.load(f)
        except Exception as e:
            self.logger.error(f"Error loading config: {str(e)}")
            raise
            
    async def save_config(self) -> None:
        """Save configuration to file"""
        try:
            with open(self.config_file, 'w') as f:
                json.dump(self.config, f, indent=4)
        except Exception as e:
            self.logger.error(f"Error saving config: {str(e)}")
            raise
            
    def get_setting(self, key: str, default: T = None) -> T:
        """Get a configuration setting"""
        return self.config.get(key, default)
        
    def set_setting(self, key: str, value: T) -> None:
        """Set a configuration setting"""
        self.config[key] = value
        
    async def save_async(self) -> None:
        """Save configuration asynchronously"""
        await self.save_config()
