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

Es existieren derzeit noch keine Migrationen und keine fachlichen API-Endpunkte. Die Anwendung legt das Datenbankschema nicht automatisch an.

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
