# Entwicklungsroadmap

## Phase 0 – Übergabefähigkeit und Architektur
- [x] Anforderungen und Entscheidungen dokumentieren
- [x] offenen Klärungsbedarf zentral erfassen
- [ ] widersprüchliche Altdokumentation vollständig bereinigen
- [ ] Architekturentscheidungen zu Historie und Merge-Regeln treffen

## Phase 1 – Backend-Grundlage
- [ ] Solution- und Projektstruktur vervollständigen
- [ ] Konfiguration und Dependency Injection sauber strukturieren
- [ ] PostgreSQL-Migrationen einrichten
- [ ] Health-/Readiness-Prüfung ergänzen
- [ ] einheitliche Fehlerantworten und Validierung
- [x] Testprojekt und CI-Build einrichten (Build erfolgreich; 13 Tests bestanden)
- [ ] Authentifizierungsverfahren und sichere erstmalige Einrichtung des ersten Administratorkontos festlegen
- [ ] Anmeldung mit persönlichen Konten und ausschließlich der Rolle `Administrator` implementieren
- [x] Zentrale `AuditLog`-Entität und EF-Core-Tabellenmodell anlegen (genau eine Zeile je erfolgreicher Aktion)
- [x] Zentralen Auditlog-Schreibdienst implementieren und registrieren (ein Eintrag je Aktion; kein separates Speichern)
- [x] Auditlog-Einträge gegen nachträgliches Ändern und Löschen schützen
- [x] Initialmigration für Artikel, Standorte, Materialposten und die einzige zentrale AuditLog-Tabelle erstellen
- [x] EF-Core-Designzeitfactory und CI-Prüfung auf ausstehende Modelländerungen ergänzen
- [ ] Migration gegen eine echte PostgreSQL-Instanz anwenden und Integrationstest ausführen
- [ ] Auditlog in alle fachlichen Schreibservices integrieren; Änderung und Eintrag gemeinsam transaktional speichern

## Phase 2 – Stammdaten
- [ ] Artikel CRUD
- [ ] Kategorien-Freitext und Vorschläge aus existierenden Kategorien
- [ ] Standort CRUD
- [ ] Regeln für Löschen/Archivieren umsetzen, nachdem entschieden

## Phase 3 – Bestände und Bewegungen
- [ ] Bestandsabfragen
- [ ] Zugang mit Zusammenführung passender Posten
- [ ] Verschiebung mit atomarer Quell-/Zieländerung
- [ ] Sammelentnahme als atomarer Vorgang
- [ ] Posten bei Menge 0 löschen
- [ ] Historie unabhängig vom aktuellen Materialposten erhalten
- [ ] Concurrency- und Integritätstests

## Phase 4 – Weboberfläche
- [ ] React + TypeScript Projekt
- [ ] Artikelverwaltung
- [ ] Standortverwaltung
- [ ] Bestandsübersicht je Standort
- [ ] Dialog für Zugang, Verschiebung und Sammelentnahme
- [ ] Historienansicht

## Phase 5 – Optionale Funktionen
Nur nach eigenständiger Priorisierung:
- [ ] Sollbestände und Soll-Ist-Vergleich
- [ ] Ablaufwarnungen
- [ ] Materialtausch
- [ ] Verplombung pro Standort
- [ ] regelmäßige Kontrollen pro Standort
- [ ] Nachfüllhinweise und Bestellkennzeichnung
- [ ] Backup und Wiederherstellung

## Phase 6 – Qualität und Bereitstellung
- [ ] End-to-End-Tests
- [ ] Backup-/Restore-Test
- [ ] Installations- und Betriebsdokumentation
- [ ] Abnahme mit realistischen Beispieldaten

## Phase 7 – Mobile Anwendung
Erst beginnen, wenn die Webanwendung stabil und fachlich vollständig ist. Gleicher Funktionsumfang wie Webapp.
