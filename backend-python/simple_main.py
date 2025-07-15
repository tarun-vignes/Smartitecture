from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
from typing import Dict, Any
import uvicorn

app = FastAPI(title="Smartitecture API", version="0.1.0")

# Pydantic models
class AgentRunRequest(BaseModel):
    input: str
    max_iterations: int = 10

class AgentRunResponse(BaseModel):
    result: str
    state: str
    iterations: int

# Basic endpoints
@app.get("/")
async def read_root():
    return {"message": "Smartitecture API is running!"}

@app.get("/health")
async def health_check():
    return {"status": "healthy", "service": "smartitecture-api"}

@app.get("/test")
async def test_endpoint():
    return {"message": "Test endpoint working!", "status": "ok"}

@app.get("/agent/state")
async def get_agent_state():
    return {
        "state": "idle",
        "iteration": 0,
        "memory": []
    }

@app.post("/agent/run")
async def run_agent(request: AgentRunRequest):
    # Simple mock response for now
    return AgentRunResponse(
        result=f"Processed: {request.input}",
        state="completed",
        iterations=1
    )

if __name__ == "__main__":
    print("Starting Smartitecture API...")
    print("Available endpoints:")
    print("- http://127.0.0.1:8000/")
    print("- http://127.0.0.1:8000/health")
    print("- http://127.0.0.1:8000/test")
    print("- http://127.0.0.1:8000/agent/state")
    print("- http://127.0.0.1:8000/agent/run")
    print("- http://127.0.0.1:8000/docs")
    uvicorn.run("simple_main:app", host="127.0.0.1", port=8001, reload=True)
