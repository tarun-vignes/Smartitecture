# AIPal - Your Windows AI Companion

AIPal is a modern Windows desktop application that provides an AI-powered assistant capable of natural conversation and system task automation.

## Features

- 🤖 Conversational AI interface powered by LLM
- 🖥️ System task automation (settings, shutdown, app launch)
- 🎤 Voice interaction support
- 🎨 Modern WinUI 3 interface with dark/light mode
- 🔒 Secure system interaction with proper permissions

## Requirements

- Windows 10 version 1809 or higher
- .NET 7.0 or higher
- Visual Studio 2022 with Windows App SDK workload

## Development Setup

1. Clone the repository
2. Open `AIPal.sln` in Visual Studio 2022
3. Restore NuGet packages
4. Build and run the solution

## Security

AIPal follows Microsoft's Desktop App Security Best Practices:
- Digitally signed binaries
- UAC elevation for admin tasks
- Input validation and command sanitization
- Clear permission dialogs
- Secure API access
