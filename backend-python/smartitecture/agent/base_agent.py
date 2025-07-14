from abc import ABC, abstractmethod
from typing import Dict, List, Optional, TypeVar, Generic, Any
from dataclasses import dataclass
from enum import Enum
import json
import logging

logger = logging.getLogger(__name__)

class AgentState(Enum):
    IDLE = "idle"
    THINKING = "thinking"
    ACTING = "acting"
    OBSERVING = "observing"
    COMPLETED = "completed"
    ERROR = "error"

@dataclass
class Tool:
    name: str
    description: str
    parameters: dict
    
    def to_dict(self) -> dict:
        return {
            "name": self.name,
            "description": self.description,
            "parameters": self.parameters
        }

class ToolExecutionError(Exception):
    pass

class BaseTool(ABC):
    @property
    @abstractmethod
    def name(self) -> str:
        """Name of the tool, must be unique."""
        pass
    
    @property
    @abstractmethod
    def description(self) -> str:
        """Description of what the tool does."""
        pass
    
    @property
    def parameters(self) -> dict:
        """JSON Schema for the tool's input parameters."""
        return {
            "type": "object",
            "properties": {},
            "required": []
        }
    
    @abstractmethod
    async def _execute(self, **kwargs) -> Any:
        """Execute the tool with the given parameters."""
        pass
    
    async def execute(self, **kwargs) -> Any:
        """Execute the tool with validation and error handling."""
        try:
            logger.info(f"Executing tool {self.name} with args: {kwargs}")
            result = await self._execute(**kwargs)
            logger.info(f"Tool {self.name} execution successful")
            return result
        except Exception as e:
            logger.error(f"Error executing tool {self.name}: {str(e)}")
            raise ToolExecutionError(f"Error in {self.name}: {str(e)}")

@dataclass
class AgentAction:
    tool_name: str
    tool_parameters: dict
    thought: str

@dataclass
class AgentObservation:
    observation: Any
    success: bool
    error: Optional[str] = None

class BaseAgent(ABC):
    def __init__(self, tools: List[BaseTool] = None, max_iterations: int = 10):
        self.tools = {tool.name: tool for tool in (tools or [])}
        self.max_iterations = max_iterations
        self.state = AgentState.IDLE
        self.iteration = 0
        self.memory = []
    
    def get_available_tools(self) -> List[Tool]:
        """Get list of available tools with their schemas."""
        return [
            Tool(
                name=tool.name,
                description=tool.description,
                parameters=tool.parameters
            )
            for tool in self.tools.values()
        ]
    
    async def execute_tool(self, tool_name: str, **kwargs) -> AgentObservation:
        """Execute a tool by name with the given parameters."""
        if tool_name not in self.tools:
            error_msg = f"Tool {tool_name} not found"
            logger.error(error_msg)
            return AgentObservation(
                observation=None,
                success=False,
                error=error_msg
            )
        
        try:
            tool = self.tools[tool_name]
            result = await tool.execute(**kwargs)
            return AgentObservation(
                observation=result,
                success=True
            )
        except ToolExecutionError as e:
            return AgentObservation(
                observation=None,
                success=False,
                error=str(e)
            )
    
    @abstractmethod
    async def think(self, input_data: Any) -> AgentAction:
        """Generate the next action based on current state and input."""
        pass
    
    @abstractmethod
    async def observe(self, observation: AgentObservation) -> bool:
        """Process the observation and determine if the task is complete."""
        pass
    
    async def run(self, input_data: Any) -> Any:
        """Run the agent with the given input."""
        self.state = AgentState.THINKING
        self.iteration = 0
        self.memory = []
        
        current_input = input_data
        
        while self.iteration < self.max_iterations and self.state != AgentState.COMPLETED:
            try:
                # Think: Decide the next action
                self.state = AgentState.THINKING
                action = await self.think(current_input)
                
                # Act: Execute the action
                self.state = AgentState.ACTING
                observation = await self.execute_tool(action.tool_name, **action.tool_parameters)
                
                # Observe: Process the result
                self.state = AgentState.OBSERVING
                is_complete = await self.observe(observation)
                
                if is_complete:
                    self.state = AgentState.COMPLETED
                    return self._format_result(observation.observation)
                
                current_input = observation.observation
                self.iteration += 1
                
            except Exception as e:
                self.state = AgentState.ERROR
                logger.error(f"Error in agent loop: {str(e)}", exc_info=True)
                raise
        
        self.state = AgentState.COMPLETED
        return self._format_result(None)
    
    def _format_result(self, result: Any) -> Any:
        """Format the final result of the agent's execution."""
        return {
            "result": result,
            "iterations": self.iteration,
            "state": self.state.value,
            "memory": self.memory
        }
