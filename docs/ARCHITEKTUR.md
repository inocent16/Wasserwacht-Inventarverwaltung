# Architektur

## Zielarchitektur

Die Anwendung wird als Solid-/TypeScript-Fullstack-Anwendung mit Supabase Cloud aufgebaut.

- UI und Fullstack: SolidJS 1.9 mit TanStack Start 1.x
- Routing: TanStack Router
- Server-State: TanStack Query
- Authentifizierung: Supabase Auth
- Persistenz: Supabase PostgreSQL
- Schemaänderungen: versionierte SQL-Migrationen in `supabase/migrations/`
- lokale Datenbanktests: Supabase CLI und pgTAP

Die Versions- und Migrationsentscheidung ist in `STACK-MIGRATION.md` dokumentiert.

## Verantwortlichkeiten

### Frontend
- Darstellung von Artikeln, Standorten und Beständen
- Formulare und clientseitige Eingabevalidierung
- Auslösen der auditierten RPCs
- Anzeige verständlicher Fehler
- keine fachlich maßgebliche Bestandsberechnung im Browser
- keine privilegierten Supabase-Schlüssel im Client

### TanStack-Server
- serverseitige Session-Verarbeitung über `@supabase/ssr`
- geschützte Server Functions und Server Routes
- serverseitige Benutzer- und Rollenprüfung für administrative Aktionen
- keine Duplizierung der fachlichen Bestandslogik, die in PostgreSQL liegt

### Supabase Auth
- persönliche Konten mit E-Mail-Adresse und Passwort
- Sitzungsverwaltung
- Identität für RLS und AuditLog-Zuordnung
- öffentliche Selbstregistrierung ist deaktiviert

### PostgreSQL
- maßgebliche fachliche Daten und Constraints
- Row Level Security für alle exponierten Anwendungstabellen
- transaktionale Bestandsfunktionen
- genau eine zentrale AuditLog-Tabelle
- keine separate Materialbewegungstabelle

## Tabellen

- `profiles`: Profil zur Auth-Identität; Rolle ist anfangs NULL und wird ausschließlich kontrolliert auf `administrator` gesetzt
- `articles`: Artikelstammdaten
- `locations`: frei verwaltete Standorte
- `stock_items`: aktuelle Materialposten; Menge ist eine positive ganze Zahl
- `audit_log`: einzige Historienquelle, append-only

## Datenbankzugriff und Sicherheit

RLS ist auf allen Anwendungstabellen aktiviert. Die API-Rollen erhalten für Artikeldaten, Standorte, Bestände und AuditLog nur die notwendigen SELECT-Rechte. Direkte Tabellenänderungen sind entzogen.

Artikel- und Standortänderungen erfolgen über `create_article`, `update_article`, `delete_article`, `create_location`, `update_location` und `delete_location`. Diese RPCs schreiben jeweils genau einen AuditLog-Eintrag.

Bestandsänderungen erfolgen ausschließlich über:
- `receive_stock`
- `transfer_stock`
- `remove_stock_batch`

Jede schreibende Funktion prüft innerhalb der Datenbank, ob `auth.uid()` eine freigeschaltete Administratorrolle besitzt. SECURITY DEFINER-Funktionen verwenden einen leeren `search_path`, qualifizierte Objektnamen und explizite EXECUTE-Rechte. Service-Role-/Secret-Keys gehören niemals in den Browser.

## Zusammenführung und Konkurrenz

Materialposten werden nur bei exakt gleichen Identitätsmerkmalen zusammengeführt: Artikel, Standort, Ablaufdatum, Charge und Hersteller. NULL wird nicht mit einem bekannten Wert gleichgesetzt. Dies ist ein sicherer Implementierungsstandard, bis die offenen Fachfragen endgültig bestätigt sind.

Die Materialidentität wird bei Zugängen und Verschiebungszielen mit transaktionalen Advisory Locks serialisiert. Quell-/Zielzeilen und Sammelentnahmeposten werden in deterministischer UUID-Reihenfolge gesperrt. Die Datenbank verhindert zusätzlich doppelte Identitäten mit einem Unique Index `NULLS NOT DISTINCT`.

Sammelentnahmen referenzieren konkrete Materialposten-UUIDs; es gibt keine implizite FIFO-/FEFO-Verteilung.

## Transaktionen und AuditLog

Jede RPC ist eine einzelne PostgreSQL-Transaktion. Scheitert irgendein Schritt, werden Bestandsänderung und AuditLog gemeinsam zurückgerollt. Es gibt genau einen AuditLog-Eintrag pro erfolgreicher fachlicher Aktion.

AuditLog-Einträge enthalten unveränderliche Actor-ID und E-Mail-Snapshot, Ereignistyp, vollständige Vorher-/Nachher-Snapshots und optionale Metadaten. Keine Audit-Fremdschlüssel dürfen historische Einträge mit aktuellen Stammdaten koppeln.

## Löschen

Artikel und Standorte können nur gelöscht werden, wenn keine aktuellen Materialposten darauf verweisen. Historieneinträge blockieren das Löschen nicht, weil sie die vollständigen Zustände als JSONB-Snapshots enthalten und keine FK-Kaskaden besitzen. Diese Löschregel ist vor dem Produktivstart fachlich zu bestätigen.

## Optionale Module

Optionale Funktionen wie Verplombung, Kontrollen, Ablaufwarnungen und Bestellkennzeichnung werden unabhängig und ohne Abhängigkeit des Kernbetriebs ergänzt.
