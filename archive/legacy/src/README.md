# Smartitecture Project Structure

This document outlines the reorganized structure of the Smartitecture project.

## Project Organization

The codebase has been reorganized into the following structure:

### Core Components

- **Smartitecture.Core**: Contains the core business logic and domain models
  - `/Commands`: Command pattern implementations for system operations
  - `/Network`: Network-related functionality
    - `/Handlers`: Network event handlers
    - `/Services`: Network monitoring services
    - `/Tools`: Network security tools
  - `/Security`: Security-related functionality
    - `/Handlers`: Security event handlers
    - `/Services`: Security monitoring services
    - `/Tools`: Security tools and utilities

### API Layer

- **Smartitecture.API**: Contains the API endpoints and services
  - `/Controllers`: API controllers for external access
  - `/Models`: Data transfer objects (DTOs)
  - `/Services`: API-specific services

### User Interface

- **Smartitecture.UI**: Contains the UI components
  - `/ViewModels`: MVVM view models
  - `/Views`: UI views and pages
  - `/Themes`: Styling and theming resources

### Tests

  - `/Commands`: Tests for command implementations
  - `/Security`: Tests for security components
  - `/Services`: Tests for services

## Namespace Structure

The namespace structure follows the physical organization:

- `Smartitecture.Core.*`: Core functionality
- `Smartitecture.API.*`: API functionality
- `Smartitecture.UI.*`: UI components

## Migration Notes

This reorganization maintains all original functionality while improving code organization and maintainability. No files were deleted during the reorganization process - all original files were copied to their new locations to ensure a smooth transition.
