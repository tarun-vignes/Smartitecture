# Smartitecture Python Desktop Version

A Python-based desktop application version of Smartitecture, built using PyQt5 for the UI and OpenAI's API for AI capabilities.

## Features

- Modern PyQt5-based desktop interface
- AI-powered assistance using OpenAI's API
- Persistent configuration management
- Secure authentication system
- File management capabilities
- Error handling and logging

## Requirements

- Python 3.9+
- PyQt5
- OpenAI API key
- See `requirements.txt` for full dependency list

## Installation

1. Clone the repository
2. Create a virtual environment:
```bash
python -m venv venv
source venv/bin/activate  # On Windows: venv\Scripts\activate
```

3. Install dependencies:
```bash
pip install -r requirements.txt
```

4. Set up environment variables:
```bash
echo "OPENAI_API_KEY=your_api_key_here" > .env
```

5. Run the application:
```bash
python main.py
```

## Project Structure

```
python-only/
├── smartitecture/
│   ├── core/
│   │   ├── services/
│   │   ├── interfaces/
│   │   └── ...
│   ├── ui/
│   │   ├── components/
│   │   └── ...
│   └── ai/
│       └── ...
└── requirements.txt
```

## Development

The project uses pytest for testing and coverage for test coverage reporting. Run tests with:
```bash
pytest
```

## Contributing

1. Fork the repository
2. Create your feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

[Insert license information here]
