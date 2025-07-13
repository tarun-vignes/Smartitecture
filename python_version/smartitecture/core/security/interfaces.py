from abc import ABC, abstractmethod
from typing import Optional, Dict

class IAuthenticationService(ABC):
    @abstractmethod
    async def login(self, username: str, password: str) -> bool:
        """Authenticate user"""
        pass
        
    @abstractmethod
    async def logout(self) -> None:
        """Logout user"""
        pass
        
    @abstractmethod
    async def register(self, username: str, password: str, email: str) -> bool:
        """Register new user"""
        pass
        
    @property
    @abstractmethod
    def is_authenticated(self) -> bool:
        """Check if user is authenticated"""
        pass
        
    @property
    @abstractmethod
    def current_user(self) -> Optional[Dict]:
        """Get current user information"""
        pass
