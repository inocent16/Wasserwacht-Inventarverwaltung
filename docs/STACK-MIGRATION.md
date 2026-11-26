# Architekturentscheidung: Solid, TanStack und Supabase Cloud

Stand: 26.11.2026

## Beschluss

Die Zielplattform wird eine TypeScript-Fullstack-Anwendung mit Supabase Cloud als verwalteter PostgreSQL- und Auth-Infrastruktur.

- UI: SolidJS 1.9
- Fullstack und Routing: TanStack Start 1.x / TanStack Router 1.x
- Server-State: TanStack Query 5.x
- Datenbank und Authentifizierung: Supabase Cloud
- Supabase SSR: `@supabase/ssr`
- Validierung: Zod
- Schemaänderungen: versionierte SQL-Migrationen in `supabase/migrations/`

Das bisherige ASP.NET-Core-/EF-Core-Backend ist Übergangsbestand und wird erst entfernt, wenn die Supabase-Ersatzfunktionen implementiert und getestet sind.

## Versionsstrategie

Solid 2.0 ist veröffentlicht. Die Solid-2-Kompatibilität der TanStack-Solid-Start-/Router-Pakete befindet sich zum Entscheidungsdatum jedoch noch in der 2.0-Release-Candidate-Linie. Für den ersten produktiven Unterbau verwenden wir deshalb die stabile TanStack-Start-1.x-Linie mit Solid 1.9. Ein Upgrade auf Solid 2 wird separat geprüft, sobald die dafür benötigten TanStack-Pakete stabil sind.

## Verantwortlichkeiten

### Solid und TanStack Start
- Darstellung und Interaktion
- Routing und geschützte Navigationsbereiche
- Server Functions für serverseitige Anwendungsfälle
- serverseitige Supabase-Session-Verarbeitung
- TanStack Query für Cache, Ladezustände und Aktualisierung von Server-State

### Supabase Auth
- persönliche Benutzerkonten
- Anmeldung und Sitzungsverwaltung
- Authentifizierungsidentität und Token-Ausstellung

### Supabase PostgreSQL
- maßgebliche fachliche Daten und Constraints
- Row Level Security (RLS)
- zentrale AuditLog-Tabelle
- transaktionale Datenbankfunktionen für Zugang, Verschiebung und Sammelentnahme

Einfache Stammdatenabfragen und -änderungen können über den Supabase Data API Client erfolgen, sofern die RLS-Regeln sie korrekt absichern. Fachliche Bestandsänderungen laufen ausschließlich über Datenbankfunktionen, damit alle betroffenen Zeilen und der eine AuditLog-Eintrag atomar gespeichert werden.

## Sicherheit
- RLS ist für jede über die Data API erreichbare Anwendungstabelle verpflichtend.
- Der Publishable Key darf im Browser verwendet werden, ist aber keine Berechtigung. Die Datenbank muss jede Aktion durch RLS und Constraints autorisieren.
- Secret-/Service-Role-Keys dürfen niemals mit einem `VITE_`-Präfix versehen oder in den Browser-Build übernommen werden.
- Serverseitige Admin-Operationen dürfen ausschließlich in server-only Code ausgeführt werden.
- Serverseitige Autorisierung vertraut nicht ungeprüft einem lokalen Session-Objekt. Der Benutzer wird für geschützte Vorgänge serverseitig mit Supabase Auth verifiziert.
- Benutzerbezogene SSR-Antworten und Query-Caches dürfen nicht zwischen Requests oder Benutzern geteilt werden.

## Erhaltene fachliche Regeln

Alle bestehenden Festlegungen bleiben gültig: Mengen in ganzen Stückzahlen, keine automatisch angelegten Standorte, optionale Ablaufdaten/Chargen/Hersteller, genau eine zentrale AuditLog-Tabelle, genau ein vollständiger AuditLog-Eintrag pro erfolgreicher fachlicher Aktion, keine StockMovement-Tabelle und transaktionale Bestandsänderungen.

## Noch nicht umgesetzt
- Supabase-Cloud-Projekt und Region
- Datenbankschema, RLS und SQL-Migrationen
- Supabase-Auth-Flows und initiale Administratoranlage
- Login-Kennung (E-Mail oder kontrollierter E-Mail-Alias für den bisherigen Benutzernamen)
- Inventar-CRUD und transaktionale Bestandsfunktionen
- automatisierte Datenbank- und Anwendungstests

Frankfurt (EU Central) ist die bevorzugte Region, aber noch nicht provisioniert.
