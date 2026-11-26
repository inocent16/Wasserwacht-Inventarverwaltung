# Architektur

## Zielarchitektur

Die Anwendung wird als Solid-/TypeScript-Fullstack-Anwendung mit Supabase Cloud aufgebaut.

- UI und Fullstack: SolidJS 1.9 mit TanStack Start 1.x
- Routing: TanStack Router
- Server-State: TanStack Query
- Backend-Infrastruktur: Supabase Cloud
- Authentifizierung: Supabase Auth
- Persistenz: Supabase PostgreSQL
- Schemaänderungen: versionierte SQL-Migrationen
- Betrieb: Webanwendung mit verwalteter Cloud-Datenbank

Die Versions- und Migrationsentscheidung ist in `STACK-MIGRATION.md` dokumentiert.

## Verantwortlichkeiten

### Frontend
- Darstellung von Artikeln, Standorten und Beständen
- Formulare und clientseitige Eingabevalidierung für Bedienkomfort
- Auslösen von Zugängen, Verschiebungen und Sammelentnahmen über abgesicherte Datenbankfunktionen
- Anzeige verständlicher Fehler
- keine fachlich maßgebliche Bestandsberechnung im Browser
- keine privilegierten Supabase-Schlüssel im Client

### TanStack-Server
- serverseitige Session-Verarbeitung über `@supabase/ssr`
- sichere Server Functions und Server Routes für serverseitige Anwendungsfälle
- Benutzer- und Rollenprüfung für administrative Aktionen
- keine Duplizierung der fachlichen Bestandslogik, die in PostgreSQL liegt

### Supabase Auth
- persönliche Konten und Sitzungen
- Login, Logout und Token-Refresh
- Identität für RLS und AuditLog-Zuordnung

### PostgreSQL
- maßgebliche fachliche Daten und Constraints
- Row Level Security für alle exponierten Tabellen
- transaktionale Bestandsfunktionen
- genau eine zentrale AuditLog-Tabelle als einzige Quelle der Änderungshistorie
- keine separate Materialbewegungstabelle

## Datenbankzugriff

RLS ist für alle Anwendungstabellen zu aktivieren. Browserzugriffe nutzen ausschließlich den Publishable Key und den authentifizierten Benutzerkontext. Serverseitige Admin-Schlüssel sind ausschließlich in Server Functions zulässig.

Einfache Artikel- und Standortabfragen können über den Supabase Client erfolgen. Bestandszugänge, Verschiebungen und Sammelentnahmen werden ausschließlich als PostgreSQL-Funktionen implementiert und aufgerufen. Jede solche Funktion muss Bestandsänderung und genau einen AuditLog-Eintrag in derselben Transaktion speichern.

## Transaktionen und Konsistenz

PostgreSQL-Funktionen laufen in einer Datenbanktransaktion. Schlägt eine Validierung, Bestandsänderung oder AuditLog-Schreiboperation fehl, wird die Aktion vollständig zurückgerollt.

Bei parallelen Benutzern muss verhindert werden, dass zwei gleichzeitige Entnahmen denselben verfügbaren Bestand überziehen. Die Implementierung muss Zeilensperren oder eine gleichwertige Concurrency-Strategie verwenden und mit PostgreSQL-Integrationstests abgesichert werden.

## Auditlog

Das zentrale Auditlog ist die einzige Quelle der Änderungshistorie. Jede erfolgreich abgeschlossene fachliche Aktion erzeugt genau einen Eintrag mit vollständigem Vorher- und Nachher-Zustand aller betroffenen Datensätze sowie der auslösenden Benutzer-ID.

Audit-Einträge bleiben unabhängig vom Lebenszyklus der betroffenen Artikel, Standorte und Materialposten erhalten. Es gibt keine StockMovement-Tabelle und keine separate Bewegungshistorie. Der fachliche Detailentwurf bleibt in `AUDITLOG-KONZEPT.md` maßgeblich.

## Authentifizierung und Rollen

Zum Start existiert ausschließlich die Rolle `Administrator`. Selbstregistrierung ist nicht vorgesehen; Konten werden durch einen Administrator angelegt. Die Rolle wird in einem geschützten Profil-/Rollenmodell gespeichert und durch RLS sowie serverseitige Admin-Funktionen geprüft.

Die endgültige Login-Kennung und der Bootstrap-Prozess für den ersten Administrator müssen vor Implementierung der Auth-Flows festgelegt werden. Supabase Auth unterstützt nativ E-Mail- und Telefonidentitäten; die bisherige frei wählbare Benutzername-Anmeldung ist daher nicht unverändert übertragbar.

## Optionale Module

Optionale Funktionen wie Verplombung, Kontrollen, Ablaufwarnungen und Bestellkennzeichnung werden unabhängig und ohne Abhängigkeit des Kernbetriebs ergänzt.
