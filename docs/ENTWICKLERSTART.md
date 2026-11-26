# Entwicklerstart

## Voraussetzungen

- .NET SDK passend zum Backend-Ziel-Framework (aktuell im Projekt: .NET 10)
- PostgreSQL
- Node.js LTS und npm (für das geplante React-Frontend)
- Git

## Repository-Struktur

```text
backend/
  src/Wasserwacht.Api/
    Domain/
    Data/
frontend/
docs/
```

## Backend lokal starten

1. PostgreSQL lokal starten.
2. Eine Datenbank mit dem Namen `wasserwacht_inventory` anlegen.
3. Connection String über die Umgebungsvariable `WASSERWACHT_DB` setzen oder lokal in User Secrets / Development-Konfiguration hinterlegen.
4. Im Projektverzeichnis ausführen:

```bash
cd backend/src/Wasserwacht.Api
dotnet restore
dotnet build
dotnet run
```

Health-Endpunkt: `GET /health`.

OpenAPI wird im Development-Modus bereitgestellt, sofern die Anwendung erfolgreich startet.

**Achtung:** Zum aktuellen Repository-Stand gibt es noch keine EF-Migrationen und keine automatische Datenbankinitialisierung. Vor fachlichen API-Tests müssen Migrationen erstellt und angewendet werden. Es gibt derzeit auch keine garantierte lokale Startbarkeit; die Befehle sind der vorgesehene Ablauf und müssen vom Entwickler verifiziert werden.

## Frontend

Das React-Projekt ist noch nicht angelegt. Es soll mit React, TypeScript und einem gepflegten Build-Tool (z. B. Vite) erstellt werden. Die konkrete Toolchain ist eine Implementierungsentscheidung; Versionen und Scripts müssen anschließend hier dokumentiert werden.

## Konfiguration und Geheimnisse

- Keine echten Passwörter oder produktiven Zugangsdaten committen.
- Lokale Connection Strings über User Secrets oder Umgebungsvariablen bereitstellen.
- `WASSERWACHT_DB` hat im vorhandenen Backend-Code Vorrang nach `ConnectionStrings:Inventory`-Fallback-Reihenfolge wie in `Program.cs` dokumentiert (derzeit wird zuerst die Konfiguration, danach die Umgebungsvariable gelesen).

## Tests

Für neue fachliche Regeln sind Unit- und Integrationstests vorzusehen. Bewegungen müssen insbesondere mit PostgreSQL-Integrationstests geprüft werden, weil Transaktions- und Concurrency-Verhalten datenbankabhängig ist.
