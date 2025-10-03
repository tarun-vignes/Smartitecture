# 🏗️ Smartitecture - AI-Powered Desktop Assistant

![Project Banner](https://via.placeholder.com/800x200?text=Smartitecture+AI+Desktop+App)

> **A next-generation Windows desktop application** blending AI capabilities with system utilities

## 🚀 Features

### 🤖 AI Integration
- **Azure OpenAI** - Cloud-based AI processing
- **Local Models** - Fallback when offline
- **Multi-Model LLM Service** - Switch between AI providers

### ⚙️ System Commands
- [LaunchAppCommand](cci:2://file:///c:/Users/tarun/OneDrive/Documents/GitHub/Smartitecture/src/Smartitecture.Core/Commands/LaunchAppCommand.cs:10:4-56:5) - Start Windows applications
- [ShutdownCommand](cci:2://file:///c:/Users/tarun/OneDrive/Documents/GitHub/Smartitecture/src/Smartitecture.Core/Commands/ShutdownCommand.cs:10:4-53:5) - System power management  
- [VolumeCommand](cci:2://file:///c:/Users/tarun/OneDrive/Documents/GitHub/Smartitecture/src/Smartitecture.Core/Commands/VolumeCommand.cs:17:4-77:5) - Audio control
- `OpenSettingsCommand` - Quick settings access

### 🔒 Security
- Permission management
- Real-time monitoring
- Security tools and utilities

### 🌐 Networking
- Network configuration
- Security scanning
- Connection diagnostics

## 🛠️ Tech Stack

| Category       | Technologies Used |
|----------------|-------------------|
| **Framework**  | .NET 8.0, WinUI 3 |
| **UI**         | WPF, XAML         |
| **AI**         | Azure OpenAI API  |
| **Architecture** | Clean Architecture, DI |

## 📦 Installation

### Prerequisites
- Windows 10/11 (build 19041+) 💻
- .NET 8.0 SDK ⚡
- Visual Studio 2022 🛠️ (with Windows App SDK workload)
- Windows SDK 10.0.22621.0+ 🧰

```powershell
# 1️⃣ Clone repository
git clone [https://github.com/tarun-vignes/Smartitecture.git](https://github.com/tarun-vignes/Smartitecture.git)

# 2️⃣ Navigate to project
cd Smartitecture

# 3️⃣ Restore packages
dotnet restore

# 4️⃣ Build solution
dotnet build
