# Entwicklungsroadmap

## Phase 0 – Anforderungen und Architektur
- [x] Fachliche Anforderungen und Entscheidungen dokumentieren
- [x] zentrale AuditLog-Regeln festlegen
- [x] Zielarchitektur SolidJS + TanStack + Supabase Cloud festlegen
- [x] React-Frontend durch Solid-/TanStack-Grundgerüst ersetzen
- [x] Supabase Browser- und Server-Client vorbereiten
- [ ] Supabase-Cloud-Projekt und Region verbindlich auswählen und anlegen

## Phase 1 – Supabase-Grundlage
- [ ] Datenbankschema als versionierte SQL-Migrationen erstellen
- [ ] Profile/Rollenmodell für Administratoren definieren
- [ ] RLS für alle Anwendungstabellen und Funktionen umsetzen
- [ ] Authentifizierung und sichere Erstanlage des ersten Administrators
- [ ] Admin-Funktion zum Anlegen weiterer Benutzer
- [ ] Auth-, RLS- und Migrationstests einrichten

## Phase 2 – AuditLog und Bestandslogik
- [ ] genau eine zentrale AuditLog-Tabelle umsetzen
- [ ] AuditLog gegen nachträgliches Ändern und Löschen schützen
- [ ] Artikel- und Standort-CRUD mit AuditLog
- [ ] Bestandsabfragen
- [ ] transaktionaler Zugang mit Zusammenführung passender Posten
- [ ] transaktionale Verschiebung mit atomarer Quell-/Zieländerung
- [ ] transaktionale Sammelentnahme
- [ ] Posten bei Menge 0 löschen
- [ ] Concurrency-, Rollback- und AuditLog-Tests

## Phase 3 – Weboberfläche
- [ ] Login und Logout
- [ ] geschützte Anwendungsnavigation
- [ ] Artikelverwaltung
- [ ] Standortverwaltung
- [ ] Bestandsübersicht je Standort
- [ ] Dialog für Zugang, Verschiebung und Sammelentnahme
- [ ] Historienansicht

## Phase 4 – Qualität und Bereitstellung
- [ ] End-to-End-Tests
- [ ] RLS-Sicherheitstests
- [ ] Backup-/Restore-Test
- [ ] Hosting und Deployment dokumentieren
- [ ] Abnahme mit realistischen Beispieldaten

## Phase 5 – Optionale Funktionen
Nur nach eigenständiger Priorisierung:
- [ ] Sollbestände und Soll-Ist-Vergleich
- [ ] Ablaufwarnungen
- [ ] Materialtausch
- [ ] Verplombung pro Standort
- [ ] regelmäßige Kontrollen pro Standort
- [ ] Nachfüllhinweise und Bestellkennzeichnung

## Phase 6 – Mobile Anwendung
Erst beginnen, wenn die Webanwendung stabil und fachlich vollständig ist. Gleicher fachlicher Funktionsumfang wie Webapp.
