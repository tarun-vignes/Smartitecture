import sys
import os
from pathlib import Path

# Add parent directory to Python path
parent_dir = str(Path(__file__).parent.parent)
sys.path.append(parent_dir)

# Run the main application
if __name__ == "__main__":
    import main
    main.main()
