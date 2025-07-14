from PyQt5.QtWidgets import (QWidget, QVBoxLayout, QTextEdit, QLineEdit,
                           QPushButton, QScrollArea, QFrame)
from PyQt5.QtCore import Qt
from PyQt5.QtGui import QFont

class ChatWidget(QWidget):
    def __init__(self, parent=None):
        super().__init__(parent)
        self.setup_ui()
        
    def setup_ui(self):
        # Create main layout
        layout = QVBoxLayout(self)
        
        # Create chat display area
        self.chat_display = QTextEdit()
        self.chat_display.setReadOnly(True)
        self.chat_display.setFont(QFont("Arial", 10))
        layout.addWidget(self.chat_display)
        
        # Create input area
        input_layout = QHBoxLayout()
        
        self.input_box = QLineEdit()
        self.input_box.setPlaceholderText("Type your message here...")
        input_layout.addWidget(self.input_box)
        
        self.send_button = QPushButton("Send")
        input_layout.addWidget(self.send_button)
        
        layout.addLayout(input_layout)
        
    def add_message(self, message: str, is_user: bool = False):
        """Add a message to the chat display"""
        prefix = "You: " if is_user else "AI: "
        self.chat_display.append(f"{prefix}{message}")
        self.chat_display.verticalScrollBar().setValue(
            self.chat_display.verticalScrollBar().maximum()
        )
