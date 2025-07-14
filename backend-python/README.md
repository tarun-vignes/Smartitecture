# Smartitecture Backend

This is the Python backend for the Smartitecture application, providing AI-powered functionality through a FastAPI-based REST API.

## Features

- **ReAct Agent Framework**: Implements the ReAct (Reasoning and Acting) pattern for AI agents
- **Tool Integration**: Supports dynamic tool registration and execution
- **FastAPI Backend**: High-performance API with automatic documentation
- **Asynchronous Processing**: Built with async/await for better performance
- **Configuration Management**: Environment-based configuration

## Getting Started

### Prerequisites

- Python 3.8+
- pip (Python package manager)

### Installation

1. Clone the repository
2. Create a virtual environment:
   ```bash
   python -m venv venv
   source venv/bin/activate  # On Windows: .\venv\Scripts\activate
   ```
3. Install dependencies:
   ```bash
   pip install -r requirements.txt
   ```

### Configuration

Copy the example environment file and update it with your settings:

```bash
cp .env.example .env
# Edit .env with your configuration
```

### Running the API

Start the development server:

```bash
python start_api.py
```

The API will be available at `http://127.0.0.1:8000`

## API Documentation

Once the server is running, you can access:

- Interactive API docs: http://127.0.0.1:8000/docs
- Alternative API docs: http://127.0.0.1:8000/redoc

## Project Structure

```
backend-python/
├── smartitecture/
│   ├── agent/                  # ReAct agent implementation
│   │   ├── __init__.py
│   │   ├── base_agent.py       # Base agent classes
│   │   ├── react_agent.py      # ReAct agent implementation
│   │   └── tools/              # Agent tools
│   │       ├── __init__.py
│   │       └── sample_tools.py # Example tools
│   ├── api/
│   │   ├── __init__.py
│   │   └── main.py             # FastAPI application
│   └── core/                   # Core functionality
│       └── services/           # Business logic services
├── tests/                      # Test files
├── .env                        # Environment variables
├── .gitignore
├── README.md
├── requirements.txt            # Python dependencies
└── start_api.py                # Application entry point
```

## Available Endpoints

### Health Check

- `GET /health`: Check if the API is running

### Agent Endpoints

- `GET /agent/state`: Get the current state of the agent
- `POST /agent/run`: Execute the agent with the given input

## Development

### Running Tests

```bash
pytest
```

### Code Style

This project uses `black` for code formatting and `isort` for import sorting:

```bash
black .
isort .
```

## License

[Your License Here]
