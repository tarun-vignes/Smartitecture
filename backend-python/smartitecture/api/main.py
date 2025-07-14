from fastapi import FastAPI, HTTPException, Depends, status
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from typing import Any, Dict, List, Optional
import logging
import uvicorn
import sys
import os
import asyncio

# Add the project root to the Python path
sys.path.append(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from agent.react_agent import ReActAgent, create_agent
from agent.base_agent import AgentState, AgentObservation

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)

# Initialize FastAPI app
app = FastAPI(
    title="Smartitecture API",
    description="API for the Smartitecture AI Agent",
    version="0.1.0"
)

# Add CORS middleware
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # In production, replace with specific origins
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Global agent instance
_agent = None

# Pydantic models for API requests/responses
class AgentRunRequest(BaseModel):
    input: str
    max_iterations: Optional[int] = 10

class AgentStateResponse(BaseModel):
    state: str
    iteration: int
    max_iterations: int
    available_tools: List[str]
    memory: List[Dict[str, Any]]

class AgentRunResponse(BaseModel):
    result: Any
    state: str
    iterations: int
    memory: List[Dict[str, Any]]

# Startup and shutdown events
@app.on_event("startup")
async def startup_event():
    """Initialize the agent when the application starts."""
    global _agent
    try:
        logger.info("Initializing ReAct agent...")
        _agent = await create_agent()
        logger.info("ReAct agent initialized successfully")
    except Exception as e:
        logger.error(f"Failed to initialize agent: {str(e)}")
        raise

@app.on_event("shutdown")
async def shutdown_event():
    """Clean up resources when the application shuts down."""
    logger.info("Shutting down agent...")
    # Add any cleanup code here if needed

# API endpoints
@app.get("/health", status_code=status.HTTP_200_OK)
async def health_check() -> Dict[str, str]:
    """Health check endpoint."""
    return {"status": "ok"}

@app.get("/agent/state", response_model=AgentStateResponse)
async def get_agent_state() -> AgentStateResponse:
    """Get the current state of the agent."""
    if _agent is None:
        raise HTTPException(
            status_code=status.HTTP_503_SERVICE_UNAVAILABLE,
            detail="Agent not initialized"
        )
    
    state = _agent.get_agent_state()
    return AgentStateResponse(**state)

@app.post("/agent/run", response_model=AgentRunResponse)
async def run_agent(request: AgentRunRequest) -> AgentRunResponse:
    """Run the agent with the given input."""
    if _agent is None:
        raise HTTPException(
            status_code=status.HTTP_503_SERVICE_UNAVAILABLE,
            detail="Agent not initialized"
        )
    
    try:
        # Set max iterations if provided
        if request.max_iterations is not None:
            _agent.max_iterations = request.max_iterations
        
        # Run the agent
        logger.info(f"Running agent with input: {request.input}")
        result = await _agent.run(request.input)
        
        return AgentRunResponse(**result)
    except Exception as e:
        logger.error(f"Error running agent: {str(e)}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Error running agent: {str(e)}"
        )

# Add a simple endpoint to test the agent
@app.get("/test", status_code=status.HTTP_200_OK)
async def test_agent() -> Dict[str, str]:
    """Test endpoint to verify the agent is working."""
    return {
        "message": "Smartitecture API is running!",
        "status": "ok"
    }

def start_api(host: str = "127.0.0.1", port: int = 8000, reload: bool = False):
    """Start the FastAPI server."""
    uvicorn.run(
        "smartitecture.api.main:app",
        host=host,
        port=port,
        reload=reload,
        log_level="info"
    )

if __name__ == "__main__":
    start_api(reload=True)
