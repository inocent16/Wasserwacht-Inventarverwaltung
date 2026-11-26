# Architekturentscheidung: Solid, TanStack und Supabase Cloud

Stand: 26.11.2026

## Beschluss

Die Zielplattform ist eine TypeScript-Fullstack-Anwendung mit Supabase Cloud als verwalteter PostgreSQL- und Auth-Infrastruktur.

- UI: SolidJS 1.9
- Fullstack und Routing: TanStack Start 1.x / TanStack Router 1.x
- Server-State: TanStack Query 5.x
- Datenbank und Authentifizierung: Supabase Cloud
- Supabase SSR: `@supabase/ssr`
- Validierung: Zod
- Schemaänderungen: versionierte SQL-Migrationen in `supabase/migrations/`
- Datenbanktests: pgTAP via Supabase CLI

Das bisherige ASP.NET-Core-/EF-Core-Backend ist Übergangsbestand und wird erst entfernt, wenn die Supabase-Ersatzfunktionen implementiert und getestet sind.

## Versionsstrategie

Solid 2.0 ist veröffentlicht. Die stabile TanStack-Start-1.x-Linie verwendet Solid 1.9. Wir verwenden diese stabile Kombination als produktiven Unterbau; ein Upgrade auf Solid 2 wird separat geprüft, sobald die erforderlichen TanStack-Pakete stabil sind.

## Verantwortlichkeiten

### Solid und TanStack Start
- Darstellung und Interaktion
- Routing und geschützte Navigationsbereiche
- Server Functions für serverseitige Anwendungsfälle
- serverseitige Supabase-Session-Verarbeitung
- TanStack Query für Cache, Ladezustände und Aktualisierung von Server-State

### Supabase Auth
- persönliche Benutzerkonten mit E-Mail-Adresse und Passwort
- Anmeldung und Sitzungsverwaltung
- Authentifizierungsidentität für RLS und AuditLog
- öffentliche Selbstregistrierung deaktiviert

### Supabase PostgreSQL
- maßgebliche fachliche Daten und Constraints
- Row Level Security (RLS)
- zentrale AuditLog-Tabelle
- auditiertes CRUD für Artikel und Standorte
- transaktionale Datenbankfunktionen für Zugang, Verschiebung und Sammelentnahme

## Implementierter Datenbankstand

Migrationen:
- `20261126233000_profiles_and_admin_role.sql`
- `20261126235000_inventory_schema_and_rls.sql`
- `20261127000000_inventory_write_functions.sql`

Tabellen:
- `profiles`
- `articles`
- `locations`
- `stock_items`
- `audit_log` (einzige Historienquelle)

Schreibzugriffe:
- Artikel: `create_article`, `update_article`, `delete_article`
- Standorte: `create_location`, `update_location`, `delete_location`
- Bestand: `receive_stock`, `transfer_stock`, `remove_stock_batch`

RLS ist auf den Anwendungstabellen aktiviert. API-Rollen dürfen Daten nur als berechtigter Administrator lesen; direkte Tabellen-DML-Rechte sind entzogen. Schreibfunktionen prüfen die Administratorrolle selbst und schreiben die fachliche Änderung sowie genau einen AuditLog-Eintrag in einer Datenbanktransaktion.

## Datenbankvalidierung

GitHub Actions startet eine frische lokale Supabase-Instanz mit Docker, wendet alle drei Migrationen an und führt die pgTAP-Suite aus. Die aktuelle Suite prüft:
- Tabellen, RLS-Aktivierung und Eindeutigkeit der Materialposten-Identität
- Existenz der drei Bestands-RPCs und Fehlen einer separaten `stock_movements`-Tabelle
- Zugriff für anon, authenticated ohne Administratorrolle und Administrator
- direkten Schreibschutz und append-only AuditLog
- Zugang, exakte Zusammenführung, unterschiedliche Chargen, Verschiebung und Sammelentnahme
- genau einen AuditLog-Eintrag pro erfolgreicher Aktion und keinen Eintrag bei fehlgeschlagener Entnahme

Diese Datenbank- und Frontend-CI-Prüfungen sind auf dem Migrationsbranch erfolgreich. Echte parallele Race-/Concurrency-Tests und Migrationen auf einem Cloud-Projekt stehen noch aus.

## Sicherheit
- RLS ist für jede über die Data API erreichbare Anwendungstabelle verpflichtend.
- Der Publishable Key darf im Browser verwendet werden, ist aber keine Berechtigung.
- Secret-/Service-Role-Keys dürfen niemals mit einem `VITE_`-Präfix versehen oder in den Browser-Build übernommen werden.
- SECURITY DEFINER-Funktionen nutzen einen leeren `search_path`, qualifizierte Objektnamen und eingeschränkte EXECUTE-Rechte.
- Serverseitige Autorisierung verifiziert Benutzer über Supabase Auth.
- Benutzerbezogene SSR-Antworten und Query-Caches dürfen nicht zwischen Requests oder Benutzern geteilt werden.

## Authentifizierung

Der Login verwendet E-Mail-Adresse und Passwort über Supabase Auth. Neue Auth-Profile erhalten zunächst `role = NULL`. Nur explizit freigeschaltete Administratoren erhalten Anwendungszugriff. Öffentliche Registrierung ist in `supabase/config.toml` deaktiviert und muss für das Cloud-Projekt ebenfalls im Dashboard deaktiviert bleiben.

Das erste Konto wird manuell im Supabase-Dashboard erstellt und einmalig im SQL Editor freigeschaltet. Weitere Benutzer sollen später über einen geschützten serverseitigen Admin-Einladungsprozess angelegt werden.

## Noch nicht umgesetzt / offen
- Supabase-Cloud-Projekt und Region (Frankfurt bevorzugt)
- Cloud-Migrationen anwenden und die reale Cloud-Konfiguration verifizieren
- Admin-Einladungsfunktion und Passwortänderung/-zurücksetzung
- echte parallele Concurrency-/Race-Integrationstests
- Inventaroberfläche, geschützte Navigation und Deployment
- fachliche Bestätigung der konservativen Merge- und Löschregeln
