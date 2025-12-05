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
