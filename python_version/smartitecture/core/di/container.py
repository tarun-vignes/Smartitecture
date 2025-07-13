from typing import Dict, Type, Callable, Any
from .interfaces import IBaseService
from ..services.base_service import BaseService

class ServiceContainer:
    def __init__(self):
        self._services: Dict[Type, Any] = {}
        
    def register(self, interface: Type, implementation: Type) -> None:
        """Register a service implementation"""
        if not issubclass(implementation, interface):
            raise ValueError(f"Implementation {implementation.__name__} must implement {interface.__name__}")
        self._services[interface] = implementation
        
    def resolve(self, interface: Type) -> Any:
        """Resolve a service instance"""
        if interface not in self._services:
            raise ValueError(f"No implementation registered for {interface.__name__}")
            
        implementation = self._services[interface]
        return implementation()
        
    def register_instance(self, interface: Type, instance: Any) -> None:
        """Register a singleton instance"""
        if not isinstance(instance, interface):
            raise ValueError(f"Instance must implement {interface.__name__}")
        self._services[interface] = instance
        
    def register_singleton(self, interface: Type, implementation: Type) -> None:
        """Register a singleton service"""
        instance = implementation()
        self.register_instance(interface, instance)
container = ServiceContainer()
