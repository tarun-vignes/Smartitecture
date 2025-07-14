from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from typing import Optional, List
import uvicorn
import os
import json
from pathlib import Path
import logging
from datetime import datetime

# Import our existing services
from ..core.services.configuration_service import ConfigurationService
from ..core.services.file_service import FileService
from ..core.config import Config

# Set up logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

app = FastAPI(title="Smartitecture API", version="1.0.0")

# Add CORS middleware
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Initialize services
config_service = ConfigurationService()
file_service = FileService()

# Pydantic models
class ConfigurationRequest(BaseModel):
    key: str
    value: str

class ConfigurationResponse(BaseModel):
    key: str
    value: str
    status: str

class FileRequest(BaseModel):
    path: str
    content: Optional[str] = None

class FileResponse(BaseModel):
    path: str
    content: Optional[str] = None
    exists: bool
    status: str

class ProcessRequest(BaseModel):
    input_text: str
    model: Optional[str] = "gpt-4"

class ProcessResponse(BaseModel):
    output_text: str
    model: str
    timestamp: str
    status: str

@app.on_event("startup")
async def startup_event():
    """Initialize services on startup"""
    config_service.initialize()
    file_service.initialize()
    logger.info("Smartitecture API started successfully")

@app.get("/")
async def root():
    return {"message": "Smartitecture API", "version": "1.0.0", "status": "running"}

@app.get("/health")
async def health_check():
    return {"status": "healthy", "timestamp": datetime.now().isoformat()}

# Configuration endpoints
@app.get("/api/config/{key}")
async def get_config(key: str):
    try:
        value = config_service.get_setting(key)
        return ConfigurationResponse(key=key, value=str(value), status="success")
    except Exception as e:
        logger.error(f"Error getting config {key}: {str(e)}")
        raise HTTPException(status_code=500, detail=str(e))

@app.post("/api/config")
async def set_config(request: ConfigurationRequest):
    try:
        config_service.set_setting(request.key, request.value)
        return ConfigurationResponse(key=request.key, value=request.value, status="success")
    except Exception as e:
        logger.error(f"Error setting config {request.key}: {str(e)}")
        raise HTTPException(status_code=500, detail=str(e))

@app.get("/api/config")
async def get_all_config():
    try:
        return {"config": config_service.config, "status": "success"}
    except Exception as e:
        logger.error(f"Error getting all config: {str(e)}")
        raise HTTPException(status_code=500, detail=str(e))

# File endpoints
@app.post("/api/file/read")
async def read_file(request: FileRequest):
    try:
        file_path = Path(request.path)
        content = file_service.read_file(file_path)
        return FileResponse(
            path=request.path,
            content=content,
            exists=content is not None,
            status="success"
        )
    except Exception as e:
        logger.error(f"Error reading file {request.path}: {str(e)}")
        raise HTTPException(status_code=500, detail=str(e))

@app.post("/api/file/write")
async def write_file(request: FileRequest):
    try:
        file_path = Path(request.path)
        success = file_service.create_file(file_path, request.content or "")
        return FileResponse(
            path=request.path,
            content=request.content,
            exists=success,
            status="success" if success else "failed"
        )
    except Exception as e:
        logger.error(f"Error writing file {request.path}: {str(e)}")
        raise HTTPException(status_code=500, detail=str(e))

@app.delete("/api/file/{path:path}")
async def delete_file(path: str):
    try:
        file_path = Path(path)
        success = file_service.delete_file(file_path)
        return {"path": path, "deleted": success, "status": "success"}
    except Exception as e:
        logger.error(f"Error deleting file {path}: {str(e)}")
        raise HTTPException(status_code=500, detail=str(e))

# AI Processing endpoint
@app.post("/api/process")
async def process_text(request: ProcessRequest):
    try:
        # TODO: Implement actual AI processing with OpenAI
        # For now, return a mock response
        mock_response = f"Processed: {request.input_text} (using {request.model})"
        
        return ProcessResponse(
            output_text=mock_response,
            model=request.model,
            timestamp=datetime.now().isoformat(),
            status="success"
        )
    except Exception as e:
        logger.error(f"Error processing text: {str(e)}")
        raise HTTPException(status_code=500, detail=str(e))

if __name__ == "__main__":
    uvicorn.run(app, host="127.0.0.1", port=8000, log_level="info")
