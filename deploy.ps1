#!/usr/bin/env pwsh
<#
.SYNOPSIS
  Bootstrap script — runs first-time Bicep provisioning before GitHub Actions CI/CD is wired up.
  After running this once, all subsequent deployments happen automatically via GitHub Actions on push to main.

.PREREQUISITES
  - Azure CLI installed and logged in: az login
  - Sufficient permissions:
      Contributor on rg-h2-experiments-dev
      User Access Administrator on the AI Foundry project in rg_h2_foundry
      AcrPush on h2experimentsacr

.USAGE
  .\deploy.ps1
  .\deploy.ps1 -ImageTag "abc1234"
#>

param(
    [string]$Subscription  = 'H2-Experimental',
    [string]$ResourceGroup = 'rg-h2-experiments-dev',
    [string]$BicepFile     = 'infra/main.bicep',
    [string]$ImageTag      = 'latest'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Write-Host "`n🔧  SupportAgent UI — Bootstrap Deployment" -ForegroundColor Cyan
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor DarkGray

# Set subscription context
Write-Host "`n→ Setting subscription context to '$Subscription'..."
az account set --subscription $Subscription

# Run Bicep deployment
Write-Host "`n→ Deploying Bicep to '$ResourceGroup'..."
$deployOutput = az deployment group create `
    --resource-group $ResourceGroup `
    --template-file $BicepFile `
    --parameters imageTag=$ImageTag `
    --query "properties.outputs" `
    --output json | ConvertFrom-Json

$url = $deployOutput.containerAppUrl.value
Write-Host "`n✅  Bicep deployment complete!" -ForegroundColor Green
if ($url) {
    Write-Host "🌐  Container App URL: $url" -ForegroundColor Cyan
}

Write-Host @"

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
NEXT STEPS — Configure GitHub Actions OIDC
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

1. Create an Azure AD App Registration (or reuse an existing one):
   az ad app create --display-name 'gh-supportagentui-deploy'

2. Note the appId (client ID) and tenantId from the output.

3. Create a service principal for the app:
   az ad sp create --id <appId>

4. Add a Federated Identity Credential for GitHub Actions:
   az ad app federated-credential create --id <appId> --parameters '{
     "name": "gh-main",
     "issuer": "https://token.actions.githubusercontent.com",
     "subject": "repo:predictiveindex/h2-bet-001-supportagentui:ref:refs/heads/main",
     "audiences": ["api://AzureADTokenExchange"]
   }'

5. Grant the service principal required roles:
   # Contributor on rg-h2-experiments-dev
   az role assignment create --assignee <appId> --role Contributor --scope /subscriptions/<sub>/resourceGroups/rg-h2-experiments-dev
   
   # AcrPush on shared ACR
   az role assignment create --assignee <appId> --role AcrPush --scope /subscriptions/<sub>/resourceGroups/rg-h2-experiments-dev/providers/Microsoft.ContainerRegistry/registries/h2experimentsacr
   
   # User Access Administrator on AI Foundry project (needed for RBAC role assignment in Bicep)
   az role assignment create --assignee <appId> --role "User Access Administrator" --scope /subscriptions/<sub>/resourceGroups/rg_h2_foundry/providers/Microsoft.MachineLearningServices/workspaces/H2-BET-001

6. Add these as GitHub repository secrets (Settings → Secrets and variables → Actions):
   AZURE_CLIENT_ID   = <appId>
   AZURE_TENANT_ID   = <tenantId>
   AZURE_SUBSCRIPTION_ID = <subscriptionId>

Once configured, every push to main will automatically deploy.
"@
