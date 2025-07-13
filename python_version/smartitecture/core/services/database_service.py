import sqlite3
import os
from pathlib import Path
from typing import Any, Dict, List
from .base_service import BaseService
from .interfaces import IBaseService

class DatabaseService(BaseService, IBaseService):
    def __init__(self):
        super().__init__()
        self.db_path = Settings.DB_PATH
        self.connection = None
        
    async def initialize(self) -> None:
        """Initialize the database connection"""
        await super().initialize()
        
        # Create database directory if it doesn't exist
        self.db_path.parent.mkdir(parents=True, exist_ok=True)
        
        # Create connection
        self.connection = sqlite3.connect(str(self.db_path))
        
        # Create tables if they don't exist
        self._create_tables()
        
    def _create_tables(self) -> None:
        """Create necessary database tables"""
        with self.connection:
            # Users table
            self.connection.execute('''
                CREATE TABLE IF NOT EXISTS users (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    username TEXT UNIQUE NOT NULL,
                    password_hash TEXT NOT NULL,
                    email TEXT UNIQUE NOT NULL,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                )
            ''')
            
            # Conversations table
            self.connection.execute('''
                CREATE TABLE IF NOT EXISTS conversations (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    user_id INTEGER NOT NULL,
                    title TEXT,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (user_id) REFERENCES users (id)
                )
            ''')
            
            # Messages table
            self.connection.execute('''
                CREATE TABLE IF NOT EXISTS messages (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    conversation_id INTEGER NOT NULL,
                    role TEXT NOT NULL,
                    content TEXT NOT NULL,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (conversation_id) REFERENCES conversations (id)
                )
            ''')
            
    async def shutdown(self) -> None:
        """Close the database connection"""
        if self.connection:
            self.connection.close()
        await super().shutdown()
        
    def execute_query(self, query: str, params: tuple = ()) -> List[Dict]:
        """Execute a query and return results"""
        try:
            cursor = self.connection.cursor()
            cursor.execute(query, params)
            columns = [col[0] for col in cursor.description]
            results = []
            for row in cursor.fetchall():
                results.append(dict(zip(columns, row)))
            return results
        except Exception as e:
            self.logger.error(f"Database error: {str(e)}")
            raise
            
    def execute_update(self, query: str, params: tuple = ()) -> None:
        """Execute an update query"""
        try:
            cursor = self.connection.cursor()
            cursor.execute(query, params)
            self.connection.commit()
        except Exception as e:
            self.logger.error(f"Database error: {str(e)}")
            raise
