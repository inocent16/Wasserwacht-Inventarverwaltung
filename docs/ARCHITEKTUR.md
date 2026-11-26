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
- Authentifizierung und Autorisierung sind bereits für Version 1.0 verpflichtend; persönliche Benutzerkonten
- Zum Start existiert ausschließlich die Rolle `Administrator`; jedes angelegte Konto erhält diese Rolle. Es gibt zunächst keine zusätzliche Standardbenutzerrolle oder feinere Berechtigungsstufen
- Benutzerkonten können nur durch einen angemeldeten Administrator angelegt werden; dieser stellt den neuen Benutzern ihre Zugangsdaten bereit
- Jede protokollierte Änderung wird dem angemeldeten Benutzer zugeordnet
- Das Anmeldeverfahren und die sichere erstmalige Einrichtung des ersten Administratorkontos müssen vor der Implementierung festgelegt werden

### Datenbank
- dauerhafte Stammdaten und Bestände
- ein verpflichtendes zentrales Auditlog als einzige Quelle der Änderungshistorie für alle Anwendungsbereiche
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
- `Infrastructure/`: technische Querschnittsthemen einschließlich des registrierten `AuditLogWriter`
- `Common/`: Fehlerantworten und gemeinsame API-Bausteine

## Transaktionen und Konsistenz

Jede fachliche Aktion erzeugt genau einen Eintrag in der zentralen AuditLog-Tabelle. Der `AuditLogWriter` hängt diesen Eintrag an denselben `InventoryDbContext` an und führt selbst kein `SaveChanges` aus. Der Anwendungscode speichert die fachlichen Änderungen und den einen Auditlog-Eintrag gemeinsam mit demselben `SaveChanges`; bei mehreren Speichervorgängen ist eine explizite Datenbanktransaktion zu verwenden. Schlägt ein Teil fehl, wird die gesamte Transaktion zurückgerollt.

Änderungen in anderen Anwendungsbereichen werden ebenfalls mit ihrem Auditlog-Eintrag konsistent gespeichert. Das Auditlog ist die einzige Quelle der Änderungshistorie; eine separate Materialbewegungstabelle wird nicht geführt.

Bei parallelen Benutzern muss verhindert werden, dass zwei gleichzeitige Entnahmen denselben verfügbaren Bestand überziehen. Die konkrete Strategie (z. B. optimistische Concurrency mit RowVersion/Concurrency-Token oder gezielte Sperre) ist vor Implementierung der Bewegungsservices festzulegen und mit PostgreSQL zu testen.

## Historie und Löschung

Es gibt genau eine zentrale Auditlog-Tabelle und keine separate Materialbewegungstabelle. Jede erfolgreich abgeschlossene fachliche Aktion erzeugt genau einen Eintrag, der den vollständigen Vorher- und Nachher-Zustand aller betroffenen Daten sowie den auslösenden Benutzer enthält. Damit werden auch Verschiebungen und Sammelentnahmen jeweils in einem Eintrag als Gesamtaktion dokumentiert. Der technische Tabellenentwurf steht in `AUDITLOG-KONZEPT.md`.

Das Auditlog ist die einzige Quelle der Historie. Die Bewegungsübersicht wird ausschließlich aus den passenden Ereignissen dieser Tabelle gebildet.

Audit-Einträge müssen unabhängig vom Lebenszyklus der betroffenen Datensätze erhalten bleiben. Historische Einträge dürfen daher nicht per Fremdschlüssel-Kaskade mit Artikeln, Standorten oder Materialposten gelöscht werden. Das konkrete Auditlog-Datenformat und die Referenzierungsstrategie werden vor der Implementierung festgelegt.

## Optionale Module

Optionale Funktionen (z. B. Verplombung, Kontrollen, Ablaufwarnungen, Bestellkennzeichnung) werden so angebunden, dass das Kernsystem ohne sie vollständig nutzbar bleibt. Standortbezogene Funktionen werden je Standort konfiguriert; es gibt keine Standortklassen.
