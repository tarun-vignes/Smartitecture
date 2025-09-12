import json
import logging
from typing import Any, Dict, List, Optional
from .base_agent import BaseAgent, AgentAction, AgentObservation, AgentState

logger = logging.getLogger(__name__)

class ReActAgent(BaseAgent):
    """
    A ReAct (Reasoning and Acting) agent that combines reasoning with actions.
    
    The agent follows this loop:
    1. Think: Generate reasoning and next action
    2. Act: Execute the action using a tool
    3. Observe: Process the result and update state
    4. Repeat until task is complete or max iterations reached
    """
    
    def __init__(self, tools: List[Any] = None, max_iterations: int = 10):
        super().__init__(tools, max_iterations)
        self._system_prompt = """
        You are a helpful AI assistant that can perform tasks using available tools.
        When given a task, think step by step and use the available tools to complete it.
        After each tool use, you'll receive the result which you should use to inform your next steps.
        When you have enough information to answer the user's request, summarize your findings and return them.
        """
    
    async def think(self, input_data: Any) -> AgentAction:
        """
        Generate the next action based on the current state and input.
        
        In a real implementation, this would use an LLM to decide the next action.
        For now, we'll use a simple rule-based approach.
        """
        # In a real implementation, this would use an LLM to generate thoughts and actions
        # For now, we'll use a simple rule-based approach
        
        if not isinstance(input_data, str):
            input_text = str(input_data)
        else:
            input_text = input_data
        
        # Simple rule-based action selection
        if any(op in input_text for op in ['+', '-', '*', '/', '(', ')']):
            return AgentAction(
                tool_name="calculator",
                tool_parameters={"expression": input_text},
                thought=f"Calculating the expression: {input_text}"
            )
        elif "time" in input_text.lower() or "date" in input_text.lower():
            return AgentAction(
                tool_name="get_current_time",
                tool_parameters={"format": "%Y-%m-%d %H:%M:%S"},
                thought="Getting the current date and time"
            )
        else:
            return AgentAction(
                tool_name="web_search",
                tool_parameters={"query": input_text, "max_results": 3},
                thought=f"Searching for information about: {input_text}"
            )
    
    async def observe(self, observation: AgentObservation) -> bool:
        """
        Process the observation from the last action and determine if the task is complete.
        
        Returns:
            bool: True if the task is complete, False otherwise
        """
        # In a real implementation, this would analyze the observation and decide
        # if the task is complete or if more actions are needed
        
        # For now, we'll consider the task complete if we got a successful observation
        # and it's not a web search result (which might need further processing)
        is_complete = observation.success and not isinstance(observation.observation, list)
        
        # Log the observation
        self.memory.append({
            "iteration": self.iteration,
            "observation": observation.observation,
            "success": observation.success,
            "error": observation.error
        })
        
        return is_complete
    
    def get_agent_state(self) -> Dict[str, Any]:
        """Get the current state of the agent."""
        return {
            "state": self.state.value,
            "iteration": self.iteration,
            "max_iterations": self.max_iterations,
            "available_tools": [tool.name for tool in self.tools.values()],
            "memory": self.memory
        }

# Example usage
async def create_agent() -> ReActAgent:
    """Create a ReAct agent with default tools."""
    from .tools.sample_tools import CalculatorTool, WebSearchTool, GetCurrentTimeTool

    tools = [
        CalculatorTool(),
        WebSearchTool(api_key=None),  # In a real app, you'd provide an API key
        GetCurrentTimeTool()
    ]
    agent = ReActAgent(tools=tools)
    return agent
    
    return ReActAgent(tools=tools)
