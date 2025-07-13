from .di.container import container
from .services import (
    ConfigurationService,
    FileService,
    ConfigurationService
)
from .interfaces import IBaseService

def configure_services():
    """Configure all services"""
    # Register singleton services
    container.register_singleton(ConfigurationService, ConfigurationService)
    container.register_singleton(FileService, FileService)
    
    # Initialize services
    config_service = container.resolve(ConfigurationService)
    file_service = container.resolve(FileService)
    
    # Initialize services
    await config_service.initialize()
    await file_service.initialize()
    
    return config_service, file_service
