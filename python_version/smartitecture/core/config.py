import json
import os
from typing import Dict, Any
from pathlib import Path

class ConfigManager:
    def __init__(self):
        self.config_dir = Path(os.path.expanduser("~/.smartitecture"))
        self.config_file = self.config_dir / "config.json"
        self.default_config = {
            "theme": "Dark",
            "model": "GPT-4",
            "temperature": 0.7,
            "max_tokens": 2000,
            "api_key": "",
            "version": "1.0.0"
        }
        
    def load(self) -> Dict[str, Any]:
        """Load configuration from file"""
        if not self.config_dir.exists():
            self.config_dir.mkdir(parents=True, exist_ok=True)
            
        if not self.config_file.exists():
            self.save(self.default_config)
            
        with open(self.config_file, 'r') as f:
            return json.load(f)
            
    def save(self, config: Dict[str, Any]) -> None:
        """Save configuration to file"""
        with open(self.config_file, 'w') as f:
            json.dump(config, f, indent=4)
            
    def update(self, key: str, value: Any) -> None:
        """Update a configuration value"""
        config = self.load()
        config[key] = value
        self.save(config)
        
    def get(self, key: str, default=None) -> Any:
        """Get a configuration value"""
        config = self.load()
        return config.get(key, default)
