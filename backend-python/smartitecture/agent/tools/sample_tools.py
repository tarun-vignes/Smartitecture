import asyncio
import json
from typing import Dict, Any, Optional
from datetime import datetime
from ..base_agent import BaseTool, ToolExecutionError
import logging

logger = logging.getLogger(__name__)

class CalculatorTool(BaseTool):
    @property
    def name(self) -> str:
        return "calculator"
    
    @property
    def description(self) -> str:
        return "Performs basic arithmetic calculations"
    
    @property
    def parameters(self) -> dict:
        return {
            "type": "object",
            "properties": {
                "expression": {
                    "type": "string",
                    "description": "Mathematical expression to evaluate"
                }
            },
            "required": ["expression"]
        }
    
    async def _execute(self, expression: str, **kwargs) -> float:
        try:
            # Basic security check - only allow basic arithmetic
            allowed_chars = set('0123456789+-*/.() ')
            if not all(c in allowed_chars for c in expression):
                raise ValueError("Invalid characters in expression")
                
            # Evaluate the expression
            result = eval(expression, {"__builtins__": {}}, {})
            return float(result)
        except Exception as e:
            raise ToolExecutionError(f"Calculation error: {str(e)}")

class WebSearchTool(BaseTool):
    def __init__(self, api_key: Optional[str] = None):
        super().__init__()
        self.api_key = api_key
    
    @property
    def name(self) -> str:
        return "web_search"
    
    @property
    def description(self) -> str:
        return "Searches the web for information"
    
    @property
    def parameters(self) -> dict:
        return {
            "type": "object",
            "properties": {
                "query": {
                    "type": "string",
                    "description": "Search query"
                },
                "max_results": {
                    "type": "integer",
                    "description": "Maximum number of results to return",
                    "default": 3
                }
            },
            "required": ["query"]
        }
    
    async def _execute(self, query: str, max_results: int = 3, **kwargs) -> list:
        # This is a mock implementation
        # In a real implementation, you would call a search API like Google Custom Search
        logger.info(f"Searching web for: {query}")
        await asyncio.sleep(1)  # Simulate API call
        
        # Return mock results
        return [
            {
                "title": f"Result for {query} {i+1}",
                "url": f"https://example.com/result/{i+1}",
                "snippet": f"This is a sample search result for '{query}' - result {i+1}"
            }
            for i in range(min(max_results, 3))
        ]

class GetCurrentTimeTool(BaseTool):
    @property
    def name(self) -> str:
        return "get_current_time"
    
    @property
    def description(self) -> str:
        return "Gets the current date and time"
    
    @property
    def parameters(self) -> dict:
        return {
            "type": "object",
            "properties": {
                "format": {
                    "type": "string",
                    "description": "Datetime format string (strftime format)",
                    "default": "%Y-%m-%d %H:%M:%S"
                }
            },
            "required": []
        }
    
    async def _execute(self, format: str = "%Y-%m-%d %H:%M:%S", **kwargs) -> str:
        return datetime.now().strftime(format)
