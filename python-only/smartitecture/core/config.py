import os
from pathlib import Path

# Application settings
APP_NAME = "Smartitecture"
APP_VERSION = "1.0.0"
DEFAULT_THEME = "light"

# Window settings
WINDOW_TITLE = f"{APP_NAME} v{APP_VERSION}"
WINDOW_WIDTH = 1200
WINDOW_HEIGHT = 800

# Directory paths
BASE_DIR = Path(__file__).parent.parent
CONFIG_DIR = BASE_DIR / "config"
DATA_DIR = BASE_DIR / "data"
LOGS_DIR = BASE_DIR / "logs"

# File paths
CONFIG_FILE = CONFIG_DIR / "config.json"
LOG_FILE = LOGS_DIR / "smartitecture.log"

# Logging settings
LOG_LEVEL = "INFO"

# API settings
OPENAI_API_KEY = os.getenv("OPENAI_API_KEY")
