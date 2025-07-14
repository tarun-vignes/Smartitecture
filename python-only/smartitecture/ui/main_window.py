import sys
import asyncio
from PyQt6.QtWidgets import (QMainWindow, QWidget, QVBoxLayout,
                           QPushButton, QLabel)
from PyQt6.QtCore import Qt
from PyQt6.QtGui import QIcon
from .components.chat_widget import ChatWidget

class SmartitectureMainWindow(QMainWindow):
    def __init__(self):
        super().__init__()
        self.setup_ui()
        
    def setup_ui(self):
        self.setWindowTitle(Config.WINDOW_TITLE)
        self.setGeometry(100, 100, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT)
        
        # Create main widget and layout
        main_widget = QWidget()
        self.setCentralWidget(main_widget)
        layout = QVBoxLayout(main_widget)
        
        # Create title label
        title = QLabel(Config.APP_NAME)
        title.setAlignment(Qt.AlignmentFlag.AlignCenter)
        title.setStyleSheet("font-size: 24px; font-weight: bold;")
        layout.addWidget(title)
        
        # Create chat widget
        self.chat_widget = ChatWidget()
        layout.addWidget(self.chat_widget)
        
    def closeEvent(self, event):
        """Handle window close"""
        event.accept()
