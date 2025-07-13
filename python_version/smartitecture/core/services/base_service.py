import logging
from typing import Any, Coroutine
from .interfaces import IBaseService

class BaseService(IBaseService):
    def __init__(self):
        self.logger = logging.getLogger(self.__class__.__name__)
        
    async def initialize(self) -> None:
        """Initialize the service"""
        self.logger.info(f"Initializing {self.__class__.__name__}")
        
    async def shutdown(self) -> None:
        """Shutdown the service"""
        self.logger.info(f"Shutting down {self.__class__.__name__}")
        
    async def execute(self, action: Coroutine) -> Any:
        """Execute an action with error handling"""
        try:
            return await action
        except Exception as e:
            self.logger.error(f"Error executing action: {str(e)}")
            raise
