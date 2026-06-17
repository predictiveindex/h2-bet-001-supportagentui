# SupportAgent UI

Public AI chat application built with Vue 3 + .NET 10 that connects to the **SupportAgent** Azure AI Foundry agent.

**Live:** Hosted on Azure Container Apps at `app-supportagentui` in `rg-h2-experiments-dev`.

---

## Architecture

```
Browser (Vue 3 SPA)
    │  POST /api/chat  {messages:[…]}
    ▼
.NET 10 ASP.NET Core Minimal API
    │  Bearer token (Managed Identity)
    ▼
Azure AI Foundry — SupportAgent (OpenAI Responses API)
```

- Single container: .NET serves both the API and the built Vue SPA as static files.
- Conversation history is maintained in the browser session (lost on page refresh or "New Chat").

---

## Local Development

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 22+](https://nodejs.org/)
- Azure CLI (`az login` with access to the H2-Experimental subscription)

### 1 — Start the .NET API

```powershell
cd src/SupportAgent.Api
dotnet run
# Listening on http://localhost:8080
```

> **Note:** `DefaultAzureCredential` is used for authentication. Locally this will use your Azure CLI
> login (`az login`). Your account needs the **Azure AI Developer** role on the AI Foundry project.

### 2 — Start the Vue dev server (separate terminal)

```powershell
cd src/SupportAgent.Web
npm install
npm run dev
# Opens http://localhost:5173
# /api requests are proxied to http://localhost:8080
```

---

## Build Docker Image Locally

```powershell
docker build -t supportagentui:local .
docker run -p 8080:8080 supportagentui:local
# Open http://localhost:8080
```

> The container uses Managed Identity for auth — it will only fully work when running on Azure
> with the system-assigned identity configured.

---

## Infrastructure

Bicep files are in `infra/`. The deployment is scoped to **`rg-h2-experiments-dev`** in the **H2-Experimental** subscription.

| Resource | Value |
|---|---|
| Container App | `app-supportagentui` |
| Container App Environment | `cae-h2-experiments-dev` (shared) |
| Container Registry | `h2experimentsacr.azurecr.io` (shared) |
| AI Foundry Endpoint | `https://H2-Experiments.services.ai.azure.com/...` |

### First-Time Bootstrap

Before GitHub Actions CI/CD is wired up, run:

```powershell
.\deploy.ps1
```

This deploys the Bicep (creates the Container App, sets up RBAC) and prints OIDC setup instructions.

---

## CI/CD — GitHub Actions

Every push to `main` (including merged PRs) triggers `.github/workflows/deploy.yml`, which:

1. Logs in to Azure via **OIDC** (no long-lived secrets)
2. Runs Bicep (`az deployment group create`) — keeps infra in sync
3. Builds and pushes the Docker image to `h2experimentsacr`:
   - `supportagentui:<git-sha>` — unique per commit (forces a new Container App revision)
   - `supportagentui:latest`
4. Updates `app-supportagentui` to the new SHA-tagged image

### GitHub Secrets Required

| Secret | Description |
|---|---|
| `AZURE_CLIENT_ID` | App Registration client ID |
| `AZURE_TENANT_ID` | Azure AD tenant ID |
| `AZURE_SUBSCRIPTION_ID` | H2-Experimental subscription ID |

### One-Time OIDC Setup

Run `.\deploy.ps1` after first Bicep deploy — it prints the exact `az` commands to:

1. Create the App Registration
2. Add the GitHub federated identity credential
3. Assign the required RBAC roles
4. Set the GitHub repo secrets

**Permissions required for the OIDC principal:**

| Role | Scope |
|---|---|
| `Contributor` | `rg-h2-experiments-dev` |
| `AcrPush` | `h2experimentsacr` |
| `User Access Administrator` | AI Foundry project in `rg_h2_foundry` |

---

## Project Structure

```
├── src/
│   ├── SupportAgent.Api/       .NET 10 minimal API
│   │   ├── Program.cs          Routes + static file serving
│   │   ├── Models/ChatModels.cs
│   │   └── Services/AgentService.cs
│   └── SupportAgent.Web/       Vue 3 + Vite SPA
│       └── src/
│           ├── App.vue
│           └── components/
│               ├── ChatWindow.vue
│               ├── MessageBubble.vue
│               └── ChatInput.vue
├── infra/
│   ├── main.bicep
│   └── modules/containerApp.bicep
├── .github/workflows/deploy.yml
├── Dockerfile
└── deploy.ps1
```
