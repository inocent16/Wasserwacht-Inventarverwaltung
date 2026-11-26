# Wasserwacht Inventarverwaltung

Modulare Inventar- und Materialverwaltung für eine Wasserwacht-Ortsgruppe.

## Ziel

Das verpflichtende Kernsystem verwaltet **Artikel, Standorte, Materialposten und Materialbewegungen**. Alle weiteren Funktionen sind optionale Erweiterungen und können unabhängig je Artikel oder Standort genutzt werden.

## Technologie

- Frontend: React + TypeScript
- Backend: ASP.NET Core / C#
- Datenbank: PostgreSQL
- Zunächst lokaler Betrieb; Mehrbenutzerfähigkeit architektonisch vorbereiten
- Mobile Anwendung erst nach Fertigstellung der Webanwendung, anschließend mit gleichem Funktionsumfang

## Pflichtumfang (Kernsystem)

- Artikel anlegen und verwalten
- Beliebig viele Standorte anlegen und verwalten
- Materialposten je Artikel und Standort mit Menge führen
- Ablaufdatum, Charge und Hersteller optional erfassen
- Materialzugänge, Verschiebungen und Sammelentnahmen
- Passende Materialposten sowohl bei Zugängen als auch bei Verschiebungen zusammenführen

### Fachliche Grundregeln

- Alle Standorte sind gleichberechtigt; es gibt keine Standorttypen.
- Das Lager ist der Standard-Ausgangsstandort für Entnahmen/Bewegungen und Standard-Ziel für neue Zugänge. Es ist nicht automatisch die beim App-Start geöffnete Ansicht.
- Artikel ohne Ablaufdatum werden vollständig unterstützt.
- Identische Posten werden zusammengeführt, wenn Artikel, Standort, Ablaufdatum und Chargenangaben zusammenpassen. Unterschiedliche bekannte Chargen bleiben getrennt.
- Sollbestände sind optional und werden je Artikel und Standort festgelegt; es gibt keinen globalen Mindestwert.

## Optionale Erweiterungen

Zusatzfunktionen dürfen das Kernsystem weder voraussetzen noch blockieren. Sie werden nur dort verwendet, wo sie benötigt werden.

- Ablaufwarnungen
- Soll-Ist-Warnungen
- Materialtausch (z. B. denselben Artikel mit neuem Ablaufdatum erfassen)
- Verplombung je Standort: Checkbox und Datum; beim Entfernen des Hakens nach dem Grund fragen
- Regelmäßige Kontrollen je Standort
- Nachfüllhinweise
- Bestellkennzeichnung
- Auditlog
- Backups und Wiederherstellung

Jeder Standort kann unabhängig konfigurieren, ob Verplombung, Kontrollen oder weitere Standortfunktionen genutzt werden. Es gibt keine unterschiedlichen Standortklassen.

## Entwicklungsplan

1. Datenmodell und Architektur festlegen
2. ASP.NET-Core-Backend und PostgreSQL-Anbindung
3. Artikel- und Standortverwaltung
4. Materialposten und Bestandsansichten
5. Zugänge, Verschiebungen und Sammelentnahmen
6. Zusammenführungsregeln und Validierungen
7. Optionale Funktionen schrittweise ergänzen
8. Tests und lokale Bereitstellung
9. Mobile Anwendung als letzter Entwicklungsschritt

## Status

Die Anforderungsanalyse und grundlegenden Produktentscheidungen sind erfolgt. Die Implementierung startet mit dem technischen Grundgerüst und dem verpflichtenden Kernsystem.
