from PyQt5.QtWidgets import (QWidget, QVBoxLayout, QHBoxLayout, QPushButton, 
                             QLabel, QProgressBar, QComboBox, QTabWidget)
from PyQt5.QtCore import Qt
from PyQt5.QtGui import QIcon

class ChatHistoryWidget(QWidget):
    def __init__(self):
        super().__init__()
        layout = QVBoxLayout()
        self.history = QLabel("Chat History")
        layout.addWidget(self.history)
        self.setLayout(layout)

class SettingsWidget(QWidget):
    def __init__(self):
        super().__init__()
        layout = QVBoxLayout()
        
        # Theme selection
        theme_label = QLabel("Theme:")
        self.theme_combo = QComboBox()
        self.theme_combo.addItems(["Light", "Dark", "System"])
        layout.addWidget(theme_label)
        layout.addWidget(self.theme_combo)
        
        # Model selection
        model_label = QLabel("AI Model:")
        self.model_combo = QComboBox()
        self.model_combo.addItems(["GPT-4", "GPT-3.5", "Claude-2"])
        layout.addWidget(model_label)
        layout.addWidget(self.model_combo)
        
        self.setLayout(layout)

class StatusWidget(QWidget):
    def __init__(self):
        super().__init__()
        layout = QHBoxLayout()
        
        self.status_label = QLabel("Status: Ready")
        self.progress = QProgressBar()
        self.progress.setMaximum(100)
        
        layout.addWidget(self.status_label)
        layout.addWidget(self.progress)
        self.setLayout(layout)
