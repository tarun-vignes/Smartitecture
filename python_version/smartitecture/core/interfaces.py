from abc import ABC, abstractmethod
from typing import Any, Dict, List

class IBaseService(ABC):
    @abstractmethod
    async def initialize(self) -> None:
        """Initialize the service"""
        pass
        
    @abstractmethod
    async def shutdown(self) -> None:
        """Shutdown the service"""
        pass
        
    @abstractmethod
    async def execute(self, action: Any) -> Any:
        """Execute an action"""
        pass

class IRepository(ABC):
    @abstractmethod
    async def get_by_id(self, id: str) -> Any:
        """Get item by ID"""
        pass
        
    @abstractmethod
    async def get_all(self) -> List[Any]:
        """Get all items"""
        pass
        
    @abstractmethod
    async def add(self, item: Any) -> Any:
        """Add item"""
        pass
        
    @abstractmethod
    async def update(self, item: Any) -> Any:
        """Update item"""
        pass
        
    @abstractmethod
    async def delete(self, id: str) -> None:
        """Delete item"""
        pass
