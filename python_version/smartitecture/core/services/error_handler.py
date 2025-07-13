from typing import Dict, Any
import logging
from datetime import datetime
from .base_service import BaseService

class ErrorHandler(BaseService):
    def __init__(self):
        super().__init__()
        self.logger = logging.getLogger(self.__class__.__name__)
        
    def handle_error(self, error: Exception, context: Dict[str, Any]) -> None:
        """Handle an error with context"""
        self.logger.error(
            f"Error in {context.get('component', 'Unknown')}: {str(error)}",
            extra={
                'context': context,
                'timestamp': datetime.now().isoformat()
            }
        )
        
    def log_error(self, error: Exception, context: Dict[str, Any]) -> None:
        """Log an error with context"""
        self.handle_error(error, context)
        
    def format_error(self, error: Exception) -> str:
        """Format an error message"""
        return f"Error: {str(error)}\nType: {type(error).__name__}"
        
    def get_error_details(self, error: Exception) -> Dict[str, Any]:
        """Get detailed error information"""
        return {
            'type': type(error).__name__,
            'message': str(error),
            'timestamp': datetime.now().isoformat(),
            'stack_trace': error.__traceback__
        }
