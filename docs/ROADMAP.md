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
- [x] Testprojekt und CI-Build einrichten (Build erfolgreich; 4 Modelltests bestanden)
- [ ] Authentifizierungsverfahren und sichere erstmalige Einrichtung des ersten Administratorkontos festlegen
- [ ] Anmeldung mit persönlichen Konten und ausschließlich der Rolle `Administrator` implementieren
- [ ] Konkretes Datenmodell und Schreibdienst für das zentrale Auditlog festlegen; vor den ersten fachlichen Schreibendpunkten umsetzen

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
