import json
from pathlib import Path
from typing import Any, Dict
from ..config import Config

class ConfigurationService:
    def __init__(self):
        self.config_file = Config.CONFIG_DIR / "config.json"
        self.config: Dict[str, Any] = {}
        
    def initialize(self):
        """Initialize configuration service"""
        # Create config directory if it doesn't exist
        Config.CONFIG_DIR.mkdir(parents=True, exist_ok=True)
        
        # Load or create config
        if self.config_file.exists():
            with open(self.config_file, 'r') as f:
                self.config = json.load(f)
        else:
            self.config = {
                'theme': Config.DEFAULT_THEME,
                'model': 'gpt-4',
                'version': Config.APP_VERSION
            }
            self.save()
            
    def save(self):
        """Save configuration to file"""
        with open(self.config_file, 'w') as f:
            json.dump(self.config, f, indent=4)
            
    def get_setting(self, key: str, default: Any = None) -> Any:
        """Get a configuration setting"""
        return self.config.get(key, default)
        
    def set_setting(self, key: str, value: Any) -> None:
        """Set a configuration setting"""
        self.config[key] = value
        self.save()
