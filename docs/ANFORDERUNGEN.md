# Anforderungen

## Verbindlicher Kern

- Artikelverwaltung
- Standortverwaltung
- Materialposten je Artikel und Standort
- Materialbewegungen: Zugang, Verschiebung und Sammelentnahme

## Artikel

Ein Artikel besitzt eine eindeutige ID, einen Namen und eine Kategorie. Beschreibung, Hersteller und Chargenangaben sind optional. Ob ein Artikel ein Ablaufdatum hat, wird berücksichtigt; Artikel ohne Ablaufdatum sind regulär nutzbar. Eine Aktiv/Inaktiv-Eigenschaft ist nicht vorgesehen.

## Standorte

Alle Standorte sind gleichberechtigt und haben keinen Typ. Das Lager ist der Standard-Ausgangsort für Materialbewegungen und der Standard-Zielort für Zugänge. Es ist nicht automatisch die Startansicht der Anwendung.

Optionale Funktionen werden pro Standort unabhängig konfiguriert, z. B. Verplombung und regelmäßige Kontrollen. Ein Standort kann keine, eine oder mehrere Zusatzfunktionen verwenden.

## Materialposten

Ein Materialposten beschreibt Artikel, Standort, Menge sowie optional Ablaufdatum, Charge und Hersteller. Ein Artikel kann an mehreren Standorten und mit mehreren Ablaufdaten vorhanden sein.

Bei Zugängen und Verschiebungen werden passende Materialposten zusammengeführt. Posten mit unterschiedlichen bekannten Chargennummern bleiben getrennt.

## Sollbestände

Sollbestände sind optional und werden je Kombination aus Artikel und Standort festgelegt. Es gibt keinen globalen Mindestwert. Ohne Sollbestand erfolgt kein Soll-Ist-Vergleich.

## Optionale Erweiterungen

Alle folgenden Funktionen sind optional und dürfen die Kern-Lagerverwaltung nicht voraussetzen oder blockieren:

- Ablaufwarnungen
- Soll-Ist-Warnungen
- Materialtausch
- Verplombung pro Standort (Checkbox und Datum; beim Entfernen des Hakens Grund abfragen)
- Regelmäßige Kontrollen pro Standort
- Nachfüllhinweise
- Bestellkennzeichnung
- Auditlog
- Backup und Wiederherstellung

Der Materialtausch soll ausgehend von einem vorhandenen Materialposten gestartet werden. Häufig wird derselbe Artikel mit einem neuen Ablaufdatum eingetragen. Die konkrete Umsetzung wird während der Implementierung entschieden.

## Technik und Plattform

- Webanwendung
- Frontend: React + TypeScript
- Backend: ASP.NET Core
- Datenbank: PostgreSQL
- Zunächst lokaler Betrieb
- Mehrbenutzerfähigkeit architektonisch vorbereiten
- Mobile Anwendung erst nach der Webanwendung; gleicher Funktionsumfang

## Nicht für den ersten Kernumfang vorgesehen

- QR-Codes
- Datenimport
- Standorttypen
- Aktiv/Inaktiv-Status für Artikel
- Separate Einsatznachbereitung (es gibt die Sammelentnahme)
