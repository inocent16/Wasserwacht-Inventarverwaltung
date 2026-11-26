# Entwicklungsroadmap

## Phase 0 – Anforderungen und Architektur
- [x] Fachliche Anforderungen und Entscheidungen dokumentieren
- [x] zentrale AuditLog-Regeln festlegen
- [x] Zielarchitektur SolidJS + TanStack + Supabase Cloud festlegen
- [x] React-Frontend durch Solid-/TanStack-Grundgerüst ersetzen
- [x] Supabase Browser- und Server-Client vorbereiten
- [x] E-Mail-/Passwort-Login und Session-Prüfung vorbereiten
- [x] Supabase CLI-Konfiguration und Signup-Deaktivierung festlegen

## Phase 1 – Supabase-Grundlage
- [x] Profile-/Rollenmodell für Administratoren definieren
- [x] Artikel-, Standort-, Materialposten- und AuditLog-Tabellen erstellen
- [x] RLS für die Anwendungstabellen umsetzen
- [x] Direkte Tabellen-DML-Rechte für API-Rollen entziehen
- [x] Supabase-CLI und pgTAP-CI einrichten
- [ ] Migrationen lokal gegen Docker-Supabase ausführen und CI grün bestätigen
- [ ] Cloud-Projekt und Region verbindlich auswählen und anlegen
- [ ] Cloud-Migrationen nach Review anwenden
- [ ] Admin-Einladungsfunktion und Passwort-Reset implementieren

## Phase 2 – AuditLog und Bestandslogik
- [x] genau eine zentrale AuditLog-Tabelle umsetzen
- [x] AuditLog gegen nachträgliches Ändern und Löschen schützen
- [x] Artikel- und Standort-CRUD über auditiertes RPC
- [x] transaktionaler Zugang mit konservativer Merge-Identität
- [x] transaktionale Verschiebung mit atomarer Quell-/Zieländerung
- [x] transaktionale Sammelentnahme konkreter Materialposten
- [x] Posten bei Menge 0 löschen
- [x] Concurrency-Sperren und Datenbank-Constraints ergänzen
- [ ] fachliche Merge-/Löschregeln final bestätigen
- [ ] pgTAP-Verhaltenstests für Bestand, AuditLog, Rollback und Concurrency ergänzen

## Phase 3 – Weboberfläche
- [x] SolidJS + TanStack Start-Grundgerüst
- [x] E-Mail-/Passwort-Login-UI
- [ ] geschützte Anwendungsnavigation
- [ ] Artikelverwaltung
- [ ] Standortverwaltung
- [ ] Bestandsübersicht je Standort
- [ ] Dialog für Zugang, Verschiebung und Sammelentnahme
- [ ] Historienansicht
- [ ] TanStack Table, Filter und Pagination integrieren

## Phase 4 – Qualität und Bereitstellung
- [ ] End-to-End-Tests
- [ ] RLS-Sicherheitstests mit anon/authenticated/admin
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
