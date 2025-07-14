from smartitecture.core.services.configuration_service import ConfigurationService
from smartitecture.core.services.file_service import FileService

def configure_services():
    """Configure and initialize services"""
    config_service = ConfigurationService()
    file_service = FileService()
    
    # Initialize services
    config_service.initialize()
    file_service.initialize()
    
    return config_service, file_service
