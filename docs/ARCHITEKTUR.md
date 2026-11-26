# Architektur

## Zielarchitektur

Die Anwendung wird als Webanwendung mit getrenntem Frontend und Backend umgesetzt.

- Frontend: React + TypeScript
- Backend: ASP.NET Core Web API (C#)
- Persistenz: PostgreSQL
- ORM: Entity Framework Core mit Npgsql
- Betrieb zunächst lokal
- spätere mobile Clients verwenden dieselbe API und denselben fachlichen Funktionsumfang

## Verantwortlichkeiten

### Frontend
- Darstellung von Artikeln, Standorten und Beständen
- Formulare und Eingabevalidierung für Bedienkomfort
- Auslösen von Zugängen, Verschiebungen und Sammelentnahmen
- Anzeige verständlicher API-Fehler
- keine fachlich maßgebliche Bestandsberechnung im Browser

### Backend
- zentrale fachliche Validierung
- Bestandsänderungen und Protokollierung aller Änderungen im zentralen Auditlog
- Transaktionsgrenzen
- Zusammenführungsregeln für Materialposten
- Schutz vor negativen Beständen und konkurrierenden Änderungen
- spätere Authentifizierung und Autorisierung

### Datenbank
- dauerhafte Stammdaten und Bestände
- ein zentrales Auditlog als einzige Quelle der Änderungshistorie für alle Anwendungsbereiche
- keine separate Materialbewegungstabelle
- relationale Integritätsbedingungen
- Transaktionen für alle mehrteiligen Bestandsänderungen
- Migrationen versionieren das Schema

## Empfohlene Backend-Schichten

Die Schichten können zunächst in einem ASP.NET-Core-Projekt liegen; sie sollen trotzdem getrennte Verantwortlichkeiten haben.

- `Domain/`: Entitäten und fachliche Regeln ohne HTTP-Abhängigkeit
- `Data/`: DbContext, EF-Konfigurationen und Migrationen
- `Features/Articles/`: Artikel-Endpunkte, DTOs und Anwendungslogik
- `Features/Locations/`: Standort-Endpunkte, DTOs und Anwendungslogik
- `Features/Stock/`: Bestandsabfragen und Materialposten
- `Features/Movements/`: Zugang, Verschiebung, Sammelentnahme und Auswertung der Auditlog-Historie
- `Infrastructure/`: technische Querschnittsthemen einschließlich Auditlog-Schreibdienst
- `Common/`: Fehlerantworten und gemeinsame API-Bausteine

## Transaktionen und Konsistenz

Jede Bestandsbewegung muss als eine atomare Datenbanktransaktion ablaufen. Bestandsänderung und zugehöriger Auditlog-Eintrag werden gemeinsam gespeichert. Schlägt ein Teil fehl, wird die gesamte Transaktion zurückgerollt.

Änderungen in anderen Anwendungsbereichen werden ebenfalls mit ihrem Auditlog-Eintrag konsistent gespeichert. Das Auditlog ist die einzige Quelle der Änderungshistorie; eine separate Materialbewegungstabelle wird nicht geführt.

Bei parallelen Benutzern muss verhindert werden, dass zwei gleichzeitige Entnahmen denselben verfügbaren Bestand überziehen. Die konkrete Strategie (z. B. optimistische Concurrency mit RowVersion/Concurrency-Token oder gezielte Sperre) ist vor Implementierung der Bewegungsservices festzulegen und mit PostgreSQL zu testen.

## Historie und Löschung

Ein zentrales Auditlog protokolliert Änderungen in allen Anwendungsbereichen. Es ist die einzige Quelle der Historie; eine zusätzliche Materialbewegungstabelle ist ausdrücklich nicht vorgesehen. Die Bewegungsübersicht wird aus den relevanten Auditlog-Einträgen gebildet.

Audit-Einträge müssen unabhängig vom Lebenszyklus der betroffenen Datensätze erhalten bleiben. Historische Einträge dürfen daher nicht per Fremdschlüssel-Kaskade mit Artikeln, Standorten oder Materialposten gelöscht werden. Das konkrete Auditlog-Datenformat und die Referenzierungsstrategie werden vor der Implementierung festgelegt.

## Optionale Module

Optionale Funktionen (z. B. Verplombung, Kontrollen, Ablaufwarnungen, Bestellkennzeichnung) werden so angebunden, dass das Kernsystem ohne sie vollständig nutzbar bleibt. Standortbezogene Funktionen werden je Standort konfiguriert; es gibt keine Standortklassen.
