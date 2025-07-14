import sys
import asyncio
from PyQt5.QtWidgets import QApplication
from smartitecture.core.startup import configure_services
from smartitecture.ui.main_window import SmartitectureMainWindow

async def main():
    # Configure services
    config_service, file_service = await configure_services()
    
    # Create application
    app = QApplication(sys.argv)
    
    # Create and show main window
    window = SmartitectureMainWindow()
    window.show()
    
    # Start event loop
    sys.exit(app.exec_())

if __name__ == "__main__":
    asyncio.run(main())
