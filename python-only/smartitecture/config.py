import os
from pathlib import Path
from dotenv import load_dotenv

# Load environment variables
load_dotenv()

class Config:
    # Application settings
    APP_NAME = "Smartitecture"
    APP_VERSION = "1.0.0"
    
    # Paths
    BASE_DIR = Path(__file__).parent.parent
    CONFIG_DIR = Path(os.path.expanduser("~")) / ".smartitecture"
    LOGS_DIR = CONFIG_DIR / "logs"
    DATA_DIR = CONFIG_DIR / "data"
    
    # API settings
    OPENAI_API_KEY = os.getenv("OPENAI_API_KEY", "")
    
    # UI settings
    WINDOW_TITLE = f"{APP_NAME} v{APP_VERSION}"
    WINDOW_WIDTH = 1200
    WINDOW_HEIGHT = 800
    
    # Logging settings
    LOG_LEVEL = os.getenv("LOG_LEVEL", "INFO")
    LOG_FILE = LOGS_DIR / "smartitecture.log"
    
    # Database settings
    DB_PATH = DATA_DIR / "smartitecture.db"
