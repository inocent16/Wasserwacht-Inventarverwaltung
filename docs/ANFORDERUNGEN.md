# Anforderungen

## Verbindlicher Kern

- Artikelverwaltung
- Standortverwaltung
- Materialposten je Artikel und Standort
- Materialbewegungen: Zugang, Verschiebung und Sammelentnahme

## Artikel

Ein Artikel besitzt eine eindeutige ID, einen Namen und eine Kategorie. Beschreibung und Hersteller sind optional. Ablaufdaten werden je Materialposten geführt; Artikel ohne Ablaufdatum werden vollständig unterstützt. Hersteller und Chargennummer sind optionale Angaben am Materialposten. Eine Aktiv/Inaktiv-Eigenschaft ist nicht vorgesehen.

Kategorien sind anfangs Freitext. Bereits verwendete Kategorien sollen als Vorschläge/Auswahl erscheinen. Später soll eine verwaltbare Kategorienliste eingeführt werden.

## Standorte

Alle Standorte sind gleichberechtigt. Es gibt keine Standorttypen und keine automatische Standortanlage beim ersten Start. Es gibt keine fest eingebaute Sonderrolle für einen Standort namens „Lager“. Ob Benutzer später einen Standardstandort auswählen können, ist offen und darf nicht vorweggenommen werden.

Optionale Funktionen werden pro Standort unabhängig konfiguriert, z. B. Verplombung und regelmäßige Kontrollen. Ein Standort kann keine, eine oder mehrere Zusatzfunktionen verwenden.

## Materialposten

Ein Materialposten beschreibt Artikel, Standort, Menge sowie optional Ablaufdatum, Charge und Hersteller. Ein Artikel kann an mehreren Standorten und mit mehreren Ablaufdaten vorkommen.

Alle Artikel werden ausschließlich in Stück geführt. Mengen sind positive ganze Zahlen. Bei vollständiger Entnahme wird der Materialposten gelöscht. Die dazugehörige Bewegungshistorie bleibt erhalten.

Bei Zugängen und Verschiebungen werden passende Materialposten zusammengeführt. Posten mit unterschiedlichen bekannten Chargennummern bleiben getrennt. Das genaue Verhalten bei fehlenden Chargenangaben und die Rolle des Herstellers beim Zusammenführen sind noch offen.

## Sollbestände

Sollbestände sind optional und werden je Kombination aus Artikel und Standort festgelegt. Es gibt keinen globalen Mindestwert. Ohne Sollbestand erfolgt kein Soll-Ist-Vergleich. Ein definierter Sollbestand muss mindestens 1 betragen.

## Historie und Auditlog

- Ein zentrales Auditlog protokolliert Änderungen in allen Anwendungsbereichen und ist die einzige Quelle der Änderungshistorie.
- Jeder Eintrag enthält den vollständigen Vorher- und Nachher-Zustand, soweit für den Ereignistyp anwendbar, sowie den auslösenden Benutzer.
- Die Historie bleibt auch nach dem Löschen eines aktuellen Materialpostens erhalten.
- Die Bewegungshistorie wird aus den passenden Einträgen dieses Auditlogs dargestellt. Eine separate Materialbewegungstabelle gibt es nicht.
- Bestandsänderung und zugehöriger Auditlog-Eintrag werden gemeinsam und transaktional gespeichert.

## Benutzer und Anmeldung

- Version 1.0 besitzt persönliche Benutzerkonten mit Anmeldung.
- Zum Start gibt es ausschließlich die Rolle `Administrator`; jedes angelegte Konto erhält diese Rolle.
- Benutzerkonten werden ausschließlich durch einen angemeldeten Administrator angelegt.
- Jede reguläre Änderung erfolgt durch einen angemeldeten Benutzer und wird diesem im Auditlog zugeordnet.
- Es gibt zunächst keine öffentliche Selbstregistrierung und keine zusätzliche Standardbenutzerrolle.

## Materialbewegungen

- Zugang: Material zu einem beliebigen Standort hinzufügen.
- Verschiebung: Material von einem Standort zu einem anderen bewegen.
- Sammelentnahme: mehrere Artikelpositionen in einem Vorgang aus einem Standort entnehmen.

Bestandsänderungen und Historieneintrag müssen gemeinsam transaktional gespeichert werden. Eine Bewegung darf nicht teilweise ausgeführt werden.

## Optionale Erweiterungen

Alle folgenden Funktionen sind optional und dürfen die Kern-Lagerverwaltung nicht voraussetzen oder blockieren:

- Ablaufwarnungen
- Soll-Ist-Warnungen
- Materialtausch
- Verplombung pro Standort (Checkbox und Datum; beim Entfernen des Hakens Grund abfragen)
- Regelmäßige Kontrollen pro Standort
- Nachfüllhinweise
- Bestellkennzeichnung
- Backup und Wiederherstellung

Der Materialtausch soll ausgehend von einem vorhandenen Materialposten gestartet werden. Häufig wird derselbe Artikel mit einem neuen Ablaufdatum eingetragen. Die konkrete Umsetzung wird während der Implementierung entschieden.

## Technik und Plattform

- Webanwendung
- Frontend: React + TypeScript
- Backend: ASP.NET Core
- Datenbank: PostgreSQL
- zunächst lokaler Betrieb
- Mehrbenutzerfähigkeit architektonisch vorbereiten
- mobile Anwendung erst nach der Webanwendung; gleicher Funktionsumfang

## Nicht für den ersten Kernumfang vorgesehen

- QR-Codes
- Datenimport
- Standorttypen
- Aktiv/Inaktiv-Status für Artikel
- separate Einsatznachbereitung (es gibt die Sammelentnahme)
