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

Die EF-Core-Migrationen legen `Articles`, `Locations`, `StockItems`, genau eine zentrale `AuditLog`-Tabelle und die Kontentabelle `Users` an. Eine separate Bewegungs- oder Historientabelle gibt es nicht. Migrationen werden nicht automatisch beim Anwendungsstart ausgeführt.

## Anmeldung und erster Administrator

Die Anmeldung verwendet ein nicht persistentes, verschlüsseltes ASP.NET-Core-Authentifizierungs-Cookie (HttpOnly, Secure, SameSite=Strict, acht Stunden mit Sliding Expiration). Schreibende Auth-Endpunkte verlangen ein Antiforgery-Token im Header `X-CSRF-TOKEN`. Der Token wird über `GET /api/v1/auth/csrf` bezogen. Anmeldung: `POST /api/v1/auth/login`; aktuelle Sitzung: `GET /api/v1/auth/me`; Abmeldung: `POST /api/v1/auth/logout`. Benutzerkonten können nur angemeldete Administratoren über `POST /api/v1/auth/users` anlegen. Jedes Konto hat ausschließlich die Rolle Administrator.

Nach dem Anwenden der Migrationen wird der erste Admin interaktiv im Terminal eingerichtet:

```bash
dotnet run -- --create-first-admin
```

Benutzername und Anzeigename werden abgefragt; das Passwort wird verdeckt eingegeben und muss mindestens 14 Zeichen haben. Das Verfahren funktioniert nur, solange noch kein Konto existiert. Zugangsdaten werden nicht als Kommandozeilenargument oder in der Konfiguration übergeben. Der Bootstrap schreibt genau einen Eintrag `InitialAdministratorCreated` in das zentrale AuditLog. Für den lokalen Betrieb ist HTTPS erforderlich, da die Authentifizierungs- und Antiforgery-Cookies ausschließlich Secure gesetzt werden.

## Tests

Die Tests liegen unter `tests/Wasserwacht.Api.Tests`. Modell- und Diensttests prüfen Datenregeln, die zentrale AuditLog-Tabelle, den Schreibdienst und den Schutz gegen nachträgliches Ändern/Löschen ohne Datenbankverbindung. Ein zusätzlicher Migrationstest benötigt eine separate, wegwerfbare PostgreSQL-Datenbank; GitHub Actions startet dafür einen kurzlebigen PostgreSQL-17-Dienst.

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
