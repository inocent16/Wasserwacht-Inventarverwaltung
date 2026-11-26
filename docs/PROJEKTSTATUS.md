# Projektstatus und Entwicklerübergabe

Stand: 26.11.2026  
Ziel: Version 1.0

## Kurzüberblick

Es entsteht eine lokale Webanwendung zur Verwaltung des Materials einer Wasserwacht-Ortsgruppe. Der verbindliche Kern besteht aus Artikelverwaltung, Standortverwaltung, Materialposten und Materialbewegungen. Optionale Funktionen dürfen diesen Kern nicht voraussetzen oder blockieren.

## Aktueller technischer Stand

Vorhanden:
- ASP.NET-Core-Webprojekt unter `backend/src/Wasserwacht.Api`
- Entity-Framework-Core-/Npgsql-Abhängigkeiten
- Domainklassen für Artikel, Standorte und Materialposten
- zentrale `AuditLogEntry`-Entität und genau eine `AuditLog`-Tabelle im EF-Core-Modell
- EF-Core-DbContext und einfacher Health-Endpunkt
- grundlegende Anforderungen und Entscheidungen in `docs/`

Bereits vorhanden:
- Initialmigration `20261126203000_InitialInventorySchema` und Migration für Benutzerkonten
- EF-Core-Designzeitfactory und CI-Prüfung auf Abweichungen zwischen Modell und Snapshot
- PostgreSQL-Integrationstest in CI
- zentraler `AuditLogWriter`; ergänzt Einträge am gemeinsamen DbContext ohne separat zu speichern
- Auditlog-Schutz im DbContext gegen Änderungen und Löschungen
- Administrator-Anmeldung mit Session-Cookie, CSRF-Schutz, Login-Rate-Limit und einmaliger interaktiver Ersteinrichtung
- React-/TypeScript-Frontend mit Login, Logout, Session-Prüfung und Vite-API-Proxy
- getrennte GitHub-Actions-Workflows für Backend und Frontend

Noch nicht vorhanden:
- fachliche Services und Validierungen
- CRUD-Endpunkte für Artikel und Standorte
- Bestandsabfragen und Bestandsänderungen (Zugang, Verschiebung, Sammelentnahme)
- Integration des AuditLogWriter in fachliche Schreibservices; dadurch noch keine produktive Transaktionskette für Materialaktionen
- vollständige Inventaroberfläche im Frontend
- Docker-/lokale Startkonfiguration

**Wichtig:** Der vorhandene Code ist weiterhin ein technisches Grundgerüst, keine lauffähige Produktversion. Das EF-Core-Modell enthält die zentrale `AuditLog`-Tabelle; der Build und alle 13 vorhandenen Tests laufen in GitHub Actions erfolgreich. Eine echte PostgreSQL-Integration der Migration und der Transaktionskette, fachliche API-Endpunkte und vollständige Anwendungstests existieren noch nicht.

## Verbindlicher Produktumfang

1. Artikel anlegen, anzeigen, ändern und löschen (Löschregeln noch fachlich festzulegen).
2. Standorte anlegen, anzeigen, ändern und löschen (Löschregeln noch fachlich festzulegen).
3. Materialposten je Artikel und Standort verwalten.
4. Materialzugänge erfassen.
5. Material zwischen Standorten verschieben.
6. Mehrere Artikel in einer Sammelentnahme aus einem Standort ausbuchen.
7. Historische Bewegungsinformationen bleiben erhalten, auch wenn ein aktueller Materialposten vollständig aufgebraucht und gelöscht wird.

Alle Mengen sind ganze Zahlen in der Einheit Stück.

## Leitplanken für die Umsetzung

- Keine automatische Anlage eines Standorts beim ersten Start.
- Keine fest eingebauten Standorttypen.
- Kein fest eingebauter Standort namens „Lager“ und keine implizite Standardstandort-Logik, solange diese nicht ausdrücklich entschieden ist.
- Artikel ohne Ablaufdatum müssen unterstützt werden.
- Identische Bestände werden bei Zugang und Verschiebung zusammengeführt.
- Unterschiedliche bekannte Chargennummern bleiben getrennt.
- Vollständig entnommene Materialposten werden gelöscht; historische Bewegungsinformationen bleiben erhalten.
- Mehrbenutzerfähigkeit von Anfang an architektonisch berücksichtigen.
- Anmeldung und Autorisierung bereits vor den fachlichen Schreibendpunkten einplanen; zum Start ausschließlich Rolle Administrator.
- Das zentrale Auditlog ist Pflichtbestandteil und einzige Historienquelle, keine optionale Erweiterung.
- Optionale Funktionen modular und unabhängig halten.

## Reihenfolge

Siehe `ROADMAP.md`. Die erste technische Lieferung soll ein durch Tests abgesichertes Backend-Fundament einschließlich Authentifizierung, Administratorkonto, zentralem Auditlog und anschließender Stammdaten-/Bestandslogik sein. Danach folgt die Weboberfläche.
