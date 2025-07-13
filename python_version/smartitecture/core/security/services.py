import hashlib
import secrets
from typing import Optional, Dict
from .interfaces import IAuthenticationService
from ...core.services.base_service import BaseService

class AuthenticationService(BaseService, IAuthenticationService):
    def __init__(self):
        super().__init__()
        self._users: Dict[str, Dict] = {}
        self._current_user: Optional[Dict] = None
        
    async def login(self, username: str, password: str) -> bool:
        """Authenticate user"""
        user = self._users.get(username)
        if not user:
            return False
            
        salt = user.get('salt')
        if not salt:
            return False
            
        hashed_password = hashlib.pbkdf2_hmac(
            'sha256',
            password.encode('utf-8'),
            salt.encode('utf-8'),
            100000
        ).hex()
        
        if hashed_password == user.get('password'):
            self._current_user = user
            return True
            
        return False
        
    async def logout(self) -> None:
        """Logout user"""
        self._current_user = None
        
    async def register(self, username: str, password: str, email: str) -> bool:
        """Register new user"""
        if username in self._users:
            return False
            
        salt = secrets.token_hex(16)
        hashed_password = hashlib.pbkdf2_hmac(
            'sha256',
            password.encode('utf-8'),
            salt.encode('utf-8'),
            100000
        ).hex()
        
        self._users[username] = {
            'username': username,
            'email': email,
            'password': hashed_password,
            'salt': salt
        }
        return True
        
    @property
    def is_authenticated(self) -> bool:
        """Check if user is authenticated"""
        return self._current_user is not None
        
    @property
    def current_user(self) -> Optional[Dict]:
        """Get current user information"""
        return self._current_user
