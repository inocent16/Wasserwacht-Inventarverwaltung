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

Noch nicht vorhanden:
- Datenbankmigrationen
- fachliche Services und Validierungen
- CRUD- und Bewegungsendpunkte
- Authentifizierung und Benutzerverwaltung (für Version 1.0 verpflichtend; zunächst ausschließlich Rolle Administrator)
- zentraler `AuditLogWriter`, registriert in Dependency Injection; fügt je fachlicher Aktion genau einen Eintrag zum gemeinsamen DbContext hinzu und speichert nicht separat
- Schutz im DbContext: Auditlog-Einträge können nach dem Anlegen nicht geändert oder gelöscht werden
- Initialmigration `20261126203000_InitialInventorySchema` für `Articles`, `Locations`, `StockItems` und die einzige zentrale `AuditLog`-Tabelle; EF-Core-Modellsnapshot vorhanden
- EF-Core-Designzeitfactory und CI-Prüfung auf Abweichungen zwischen Modell und Snapshot
- Migration wurde noch nicht gegen eine echte PostgreSQL-Instanz angewendet
- Integration des Schreibdiensts in die noch nicht vorhandenen fachlichen Schreibservices; dadurch noch keine produktive Transaktionskette
- React-Frontend
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
