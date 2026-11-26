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
- Bestandsänderungen und Bewegungsprotokollierung
- Transaktionsgrenzen
- Zusammenführungsregeln für Materialposten
- Schutz vor negativen Beständen und konkurrierenden Änderungen
- spätere Authentifizierung und Autorisierung

### Datenbank
- dauerhafte Stammdaten und Bestände
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
- `Features/Movements/`: Zugang, Verschiebung, Sammelentnahme und Historie
- `Infrastructure/`: technische Querschnittsthemen
- `Common/`: Fehlerantworten und gemeinsame API-Bausteine

## Transaktionen und Konsistenz

Jede Bestandsbewegung muss als eine atomare Datenbanktransaktion ablaufen. Bestand und dazugehöriger Historieneintrag werden gemeinsam gespeichert. Schlägt ein Teil fehl, wird die gesamte Transaktion zurückgerollt.

Bei parallelen Benutzern muss verhindert werden, dass zwei gleichzeitige Entnahmen denselben verfügbaren Bestand überziehen. Die konkrete Strategie (z. B. optimistische Concurrency mit RowVersion/Concurrency-Token oder gezielte Sperre) ist vor Implementierung der Bewegungsservices festzulegen und mit PostgreSQL zu testen.

## Historie und Löschung

Fachlich bestätigt ist: Ein Materialposten wird bei vollständiger Entnahme gelöscht, seine Bewegungshistorie bleibt erhalten.

Die bereitgestellte Projektübergabe beschreibt ein zentrales Auditlog und sieht keine separate Materialbewegungstabelle vor. Im aktuellen Code existiert dagegen bereits eine `StockMovement`-Entität. Ob diese als fachliche Bewegungshistorie bestehen bleibt oder durch ein zentrales Auditlog ersetzt wird, ist als offene Architekturentscheidung dokumentiert. Bis zur Klärung keine irreversible Schema-Migration darauf aufbauen.

## Optionale Module

Optionale Funktionen (z. B. Verplombung, Kontrollen, Ablaufwarnungen, Bestellkennzeichnung) werden so angebunden, dass das Kernsystem ohne sie vollständig nutzbar bleibt. Standortbezogene Funktionen werden je Standort konfiguriert; es gibt keine Standortklassen.
