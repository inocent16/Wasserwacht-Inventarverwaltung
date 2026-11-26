# Architekturentscheidung: Solid, TanStack und Supabase Cloud

Stand: 27.11.2026

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
- öffentliche Selbstregistrierung deaktiviert (Cloud-Einstellung noch manuell zu bestätigen)

### Supabase PostgreSQL
- maßgebliche fachliche Daten und Constraints
- Row Level Security (RLS)
- zentrale AuditLog-Tabelle
- auditiertes CRUD für Artikel und Standorte
- transaktionale Datenbankfunktionen für Zugang, Verschiebung und Sammelentnahme

## Supabase-Cloud-Projekt

- Name: `wasserwacht-inventarverwaltung`
- Project Ref: `mqzdlvvzawaoidgajuho`
- Organisation: `inno16's org`
- Region: Frankfurt / EU Central (`eu-central-1`)
- URL: `https://mqzdlvvzawaoidgajuho.supabase.co`
- zuletzt geprüfter Status: `ACTIVE_HEALTHY`

Das Projekt wurde frisch angelegt. Es enthielt vor dem Setup keine Tabellen oder Migrationen und keine Produktdaten. Alle vier Repo-Migrationen wurden erfolgreich angewendet. Anschließend wurden Tabellen, Spalten, Foreign Keys, RLS und Rollenrechte direkt in der Cloud geprüft.

## Implementierter Datenbankstand

Migrationen:
- `20261126233000_profiles_and_admin_role.sql`
- `20261126235000_inventory_schema_and_rls.sql`
- `20261127000000_inventory_write_functions.sql`
- `20261127000500_restrict_service_role_inventory_access.sql`

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

RLS ist auf allen fünf Tabellen aktiviert. `authenticated` darf Daten nur als berechtigter Administrator lesen. Direkte Tabellenrechte sind auch für `service_role` entzogen; der Secret-Key kann die Inventar-RPCs nicht ausführen. Cloud-Privilege-Checks bestätigten: `service_role` kann Inventartabellen weder lesen noch schreiben und `receive_stock` nicht ausführen; `anon` kann die Bestands-RPCs nicht ausführen. `authenticated` darf die vorgesehenen Bestands-RPCs ausführen, die intern Administratorrechte verlangen. Der Audit-Writer ist für `authenticated` nicht ausführbar.

## Datenbankvalidierung

GitHub Actions startet eine frische lokale Supabase-Instanz mit Docker, wendet alle vier Migrationen an und führt die pgTAP-Suite aus. Die aktuelle Suite prüft:
- Tabellen, RLS-Aktivierung und Eindeutigkeit der Materialposten-Identität
- Existenz der drei Bestands-RPCs und Fehlen einer separaten `stock_movements`-Tabelle
- Zugriff für anon, authenticated ohne Administratorrolle und Administrator
- direkten Schreibschutz und append-only AuditLog
- Zugang, exakte Zusammenführung, unterschiedliche Chargen, Verschiebung und Sammelentnahme
- genau einen AuditLog-Eintrag pro erfolgreicher Aktion und keinen Eintrag bei fehlgeschlagener Entnahme
- fehlende direkte Tabellen- und RPC-Rechte für `service_role`

Frontend-Build/TypeScript und Database-CI sind auf dem Migrationsbranch erfolgreich. Echte parallele Race-/Concurrency-Tests stehen noch aus.

## Supabase Advisor

- Health: keine Findings.
- Security: zehn Hinweise auf für `authenticated` ausführbare SECURITY DEFINER-Funktionen. Das ist für die vorgesehenen Admin-RPCs beabsichtigt: alle schreibenden RPCs prüfen zuerst `require_inventory_admin()`; Hilfsfunktionen bleiben nicht direkt ausführbar. Die Warnung ist daher kein pauschaler Beweis für eine Schwachstelle, muss aber bei jeder neuen RPC-Änderung erneut geprüft werden.
- Performance: sechs aktuell ungenutzte Indizes. Da die Cloud-Datenbank noch leer ist, zunächst erwartbar; nach realer Nutzung messen, bevor Indizes entfernt werden.

## Sicherheit
- RLS ist für jede über die Data API erreichbare Anwendungstabelle verpflichtend.
- Der Publishable Key darf im Browser verwendet werden, ist aber keine Berechtigung.
- Secret-/Service-Role-Keys dürfen niemals mit einem `VITE_`-Präfix versehen oder in den Browser-Build übernommen werden.
- SECURITY DEFINER-Funktionen nutzen einen leeren `search_path`, qualifizierte Objektnamen und eingeschränkte EXECUTE-Rechte.
- Serverseitige Autorisierung verifiziert Benutzer über Supabase Auth.
- Benutzerbezogene SSR-Antworten und Query-Caches dürfen nicht zwischen Requests oder Benutzern geteilt werden.

## Authentifizierung

Der Login verwendet E-Mail-Adresse und Passwort über Supabase Auth. Neue Auth-Profile erhalten zunächst `role = NULL`. Nur explizit freigeschaltete Administratoren erhalten Anwendungszugriff.

Die lokale Datei `supabase/config.toml` deaktiviert öffentliche Registrierung für lokale Supabase-Umgebungen. Für das Cloud-Projekt ist die Einstellung separat im Dashboard unter Authentication → General Configuration → **Allow new users to sign up** zu deaktivieren. Dieser Schritt ist noch nicht bestätigt. Vor dem ersten Benutzer muss die Cloud-Option ausgeschaltet sein.

Das erste Konto wird manuell im Supabase-Dashboard erstellt und einmalig im SQL Editor freigeschaltet. Weitere Benutzer sollen später über einen geschützten serverseitigen Admin-Einladungsprozess angelegt werden.

## TypeScript-Konfiguration

Die Cloud-Datenbanktypen wurden generiert und unter `frontend/src/lib/supabase/database.types.ts` abgelegt. Browser- und Server-Clients verwenden `SupabaseClient<Database>`. `frontend/.env.example` enthält die öffentliche Projekt-URL und den Publishable Key. Es ist kein Service-Role-Key im Frontend hinterlegt.

## Noch nicht umgesetzt / offen
- Cloud-Self-Signup deaktivieren und verifizieren
- erstes Administratorkonto anlegen und Profilrolle freischalten
- Admin-Einladungsfunktion und Passwortänderung/-zurücksetzung
- echte parallele Concurrency-/Race-Integrationstests
- Inventaroberfläche, geschützte Navigation und Deployment
- fachliche Bestätigung der konservativen Merge- und Löschregeln
