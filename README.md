# Nexus Platform

A modern .NET 9 cloud-native microservices application built using Clean Architecture, Azure Container Apps, and Azure Table Storage. The platform focuses on modular service design, testability, and cloud-first deployment using Azure Developer CLI (azd).

---

## 🚀 Key Features
- Customer Account & Address management
- Minimal API endpoints in ASP.NET Core
- Clean Architecture (Domain ➜ Application ➜ API)
- Azure Table Storage for persistence
- Cloud hosting via Azure Container Apps
- OpenTelemetry tracing
- Automated CI/CD with GitHub Actions + azd

---

## 🧱 Solution Structure (High-Level)


For full architecture, see **docs/ARCHITECTURE.md**.

---

## 🛠️ Technologies
- .NET 9 + C# (nullable enabled)
- Azure Table Storage
- Azure Container Apps
- MediatR (CQRS)
- OpenTelemetry
- xUnit / FluentAssertions / NSubstitute

Full library list: **docs/TECH_STACK.md**

---

## ▶️ Run Locally

### 1. Install dependencies
- .NET 9 SDK  
- Docker (for Azurite emulator)
- Azure Developer CLI (`azd`)

### 2. Start Azurite (local Azure Storage)
```bash
docker run -p 10000:10000 -p 10001:10001 -p 10002:10002 mcr.microsoft.com/azure-storage/azurite
```

### 3. Run the application
```bash
dotnet run --project src/Nexus.AppHost
```

---

## 🤖 AI-Assisted Development

This project is optimized for AI-assisted development with GitHub Copilot and other AI tools.

### **Automatic AI Integration**
- **GitHub Copilot** automatically reads `.github/copilot-instructions.md` 
- No manual setup required - Copilot understands your patterns out of the box
- AI suggestions follow established Clean Architecture and MediatR patterns

### **Using the Prompt Guide**
- **Reference Guide**: `docs/PROMPT_GUIDE.md` contains comprehensive templates and examples
- **Manual Reference**: Copy/paste templates when working with other AI tools (ChatGPT, Claude, etc.)
- **Training Resource**: Use patterns to train team members on project conventions

### **AI Assistance Modes**

#### 🔄 **Automatic (No Action Required)**
GitHub Copilot automatically:
- Suggests code following your Clean Architecture patterns
- Generates MediatR handlers with proper structure
- Creates API endpoints using established conventions
- Implements repository patterns with Azure Table Storage

#### 💬 **Interactive (Copilot Chat)**
Use `@workspace` commands for complex requests:
```
@workspace Create a new MediatR handler for UpdateOrder following existing patterns
@workspace Generate integration tests for the Orders API endpoints  
@workspace Refactor this code to follow Clean Architecture principles
```

#### 📋 **Template-Based (Manual Copy)**
For other AI tools, reference `docs/PROMPT_GUIDE.md`:
```
Context: Working on Nexus Platform - .NET 9 microservices with Clean Architecture

Request: Create a new domain entity for Order following Account.cs patterns
Pattern: src/Nexus.CustomerOrder.Domain/Features/Accounts/Account.cs
```

### **AI Best Practices**
✅ **GitHub Copilot**: Works automatically with project context  
✅ **VS Code Chat**: Use `@workspace` for project-aware assistance  
✅ **Other AI Tools**: Reference `docs/PROMPT_GUIDE.md` for consistency  
✅ **Code Comments**: Add references to guide AI suggestions  

### **Team Workflow**
1. **New Developers**: Read `docs/PROMPT_GUIDE.md` to understand patterns
2. **Daily Coding**: Let Copilot handle routine code generation
3. **Complex Features**: Use Copilot Chat with `@workspace` commands
4. **External AI**: Copy templates from prompt guide for consistency

---

## 📚 Documentation

- **[Architecture Guide](docs/ARCHITECTURE.md)** - Complete architectural overview
- **[Prompt Guide](docs/PROMPT_GUIDE.md)** - AI assistance templates and best practices
- **[Copilot Instructions](.github/copilot-instructions.md)** - Auto-loaded GitHub Copilot patterns

---
