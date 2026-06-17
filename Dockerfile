# Stage 1 — Build Vue SPA
FROM node:22-alpine AS web-build
WORKDIR /web
COPY src/SupportAgent.Web/package*.json ./
RUN npm ci
COPY src/SupportAgent.Web/ ./
RUN node node_modules/vite/bin/vite.js build

# Stage 2 — Publish .NET app
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS api-build
WORKDIR /src
COPY src/SupportAgent.Api/SupportAgent.Api.csproj ./
RUN dotnet restore
COPY src/SupportAgent.Api/ ./
RUN dotnet publish -c Release -o /publish

# Stage 3 — Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=api-build /publish ./
COPY --from=web-build /web/dist ./wwwroot

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "SupportAgent.Api.dll"]
