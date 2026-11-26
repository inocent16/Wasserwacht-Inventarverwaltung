# Backend

ASP.NET Core Web API mit C# und PostgreSQL (EF Core / Npgsql).

## Projekt

`src/Wasserwacht.Api` ist das API-Projekt. Das aktuelle Grundgerüst enthält Domainklassen, `InventoryDbContext`, PostgreSQL-Konfiguration, OpenAPI im Development-Modus und `GET /health`.

## Lokaler Start

Voraussetzungen: .NET 10 SDK und PostgreSQL. Datenbank `wasserwacht_inventory` anlegen und `WASSERWACHT_DB` oder `ConnectionStrings:Inventory` konfigurieren.

```bash
cd src/Wasserwacht.Api
dotnet restore
dotnet build
dotnet run
```

Die initiale EF-Core-Migration `20261126203000_InitialInventorySchema` und der Modellsnapshot sind vorhanden. Sie legen `Articles`, `Locations`, `StockItems` und genau eine zentrale `AuditLog`-Tabelle an. Die Migration wird nicht automatisch beim Anwendungsstart ausgeführt.

## Tests

Die Tests liegen unter `tests/Wasserwacht.Api.Tests`. Sie prüfen bestätigte Datenregeln, die zentrale AuditLog-Tabelle, den Schreibdienst und den Schutz gegen nachträgliches Ändern/Löschen. Die aktuellen Modell- und Diensttests benötigen keine laufende PostgreSQL-Instanz.

```bash
cd backend
dotnet restore tests/Wasserwacht.Api.Tests/Wasserwacht.Api.Tests.csproj
dotnet test tests/Wasserwacht.Api.Tests/Wasserwacht.Api.Tests.csproj --no-restore --configuration Release
```

## Fachliche Regeln

- Alle Mengen sind positive ganze Zahlen in Stück.
- Materialposten mit Menge 0 existieren nicht; bei vollständiger Entnahme wird der Posten gelöscht.
- Es gibt genau eine zentrale `AuditLog`-Tabelle als einzige Historienquelle; eine `StockMovement`-Tabelle oder separate Bewegungshistorie gibt es nicht.
- Jede erfolgreich abgeschlossene fachliche Aktion erzeugt genau einen Auditlog-Eintrag mit den vollständigen Vorher- und Nachher-Zuständen aller betroffenen Daten.
- `AuditLogWriter` hängt genau einen Eintrag für die gesamte Aktion an den gemeinsamen `InventoryDbContext` an und ruft selbst kein `SaveChanges` auf.
- Der fachliche Anwendungscode speichert die Bestandsänderungen und diesen einen Auditlog-Eintrag gemeinsam mit demselben `SaveChanges`. Bei mehreren Speichervorgängen innerhalb einer Aktion ist eine explizite Datenbanktransaktion erforderlich.
- Auditlog-Einträge sind im `InventoryDbContext` gegen nachträgliches Ändern und Löschen geschützt und bleiben unabhängig vom Lebenszyklus aktueller Datensätze erhalten.
- Version 1.0 benötigt persönliche Benutzerkonten und Anmeldung. Zum Start gibt es ausschließlich die Rolle Administrator.
- Der Browser darf Bestände nicht direkt überschreiben; Änderungen laufen über Bewegungsservices.

Siehe `../docs/ARCHITEKTUR.md`, `../docs/DOMAENENMODELL.md`, `../docs/API-KONTRAKT.md` und `../docs/OFFENE_FRAGEN.md`.

## Datenbankmigration

EF-Core-Designzeitwerkzeuge benötigen dieselbe explizite Verbindungsvariable wie die Anwendung. Es werden keine Zugangsdaten fest eingebaut.

```bash
# Eigene lokale Verbindungszeichenfolge setzen
export WASSERWACHT_DB='Host=localhost;Database=wasserwacht_inventory;Username=<user>;Password=<password>'

dotnet tool install --global dotnet-ef --version 10.0.15
dotnet ef migrations list --project src/Wasserwacht.Api/Wasserwacht.Api.csproj --startup-project src/Wasserwacht.Api/Wasserwacht.Api.csproj
dotnet ef database update --project src/Wasserwacht.Api/Wasserwacht.Api.csproj --startup-project src/Wasserwacht.Api/Wasserwacht.Api.csproj
```

Die GitHub-Actions-Pipeline prüft zusätzlich, ob das EF-Core-Modell vom gespeicherten Migrationssnapshot abweicht, und führt einen Migrationstest gegen einen kurzlebigen PostgreSQL-17-Dienst aus. Dabei wird das Schema angelegt und geprüft, dass die zentrale `AuditLog`-Tabelle existiert, JSONB verwendet wird und keine `StockMovements`-Tabelle angelegt wird. Die Migration wird nicht auf eine produktive Datenbank angewendet.
