# Smartitecture Python Version

A desktop AI assistant built with Python and PyQt5.

## Setup

1. Create a virtual environment:
```bash
python -m venv venv
source venv/bin/activate  # On Windows: venv\Scripts\activate
```

2. Install dependencies:
```bash
pip install -r requirements.txt
```

3. Create a `.env` file with your API keys:
```
OPENAI_API_KEY=your_api_key_here
```

4. Run the application:
```bash
python -m smartitecture.ui.main_window
```

## Features

- Clean, modern UI with PyQt5
- AI integration using OpenAI
- Local configuration management
- Error handling and logging

## Development

The project is organized into several modules:

- `core/`: Core functionality and utilities
- `ai/`: AI agent and processing
- `ui/`: User interface components
- `services/`: Background services
- `tests/`: Test suite
