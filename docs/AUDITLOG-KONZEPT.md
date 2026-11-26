# Zentrales Auditlog

## Verbindliche Festlegung

- Es gibt genau **eine zentrale Auditlog-Tabelle** für die gesamte Anwendung.
- Diese Tabelle ist die einzige Quelle der Änderungshistorie – auch für Materialzugänge, Verschiebungen und Sammelentnahmen.
- Es gibt keine `StockMovement`-Tabelle, keine separate Bewegungshistorie und kein zweites Protokollsystem.
- Jede erfolgreich abgeschlossene fachliche Aktion erzeugt genau einen Auditlog-Eintrag.
- Der Eintrag enthält den vollständigen Vorher- und Nachher-Zustand aller von dieser Aktion betroffenen Daten, soweit für das Ereignis anwendbar.
- Der Eintrag ordnet die Aktion dem angemeldeten Benutzer zu.
- Die fachliche Änderung und genau dieser Auditlog-Eintrag werden gemeinsam in derselben Datenbanktransaktion gespeichert.
- Auditlog-Einträge bleiben erhalten, auch wenn aktuelle Artikel, Standorte oder Materialposten später gelöscht werden.

## Beispiele für einen Eintrag je Aktion

- **Artikel ändern:** Ein Eintrag mit dem vollständigen Artikelzustand vor und nach der Änderung.
- **Zugang:** Ein Eintrag mit dem Zustand des betroffenen Materialpostens vor und nach dem Zugang. Bei einem neu angelegten Posten ist der Vorher-Zustand `null`.
- **Verschiebung:** Ein Eintrag für die gesamte Verschiebung. Der Vorher-Zustand enthält den vollständigen Quell- und Zielbestand vor der Aktion; der Nachher-Zustand enthält beide Zustände nach der Aktion.
- **Sammelentnahme:** Ein Eintrag für die gesamte Sammelentnahme. Vorher- und Nachher-Zustand enthalten alle von der Sammelentnahme betroffenen Materialposten.
- **Löschen:** Ein Eintrag mit dem vollständigen Zustand vor der Löschung und `null` als Nachher-Zustand, soweit es sich um einen einzelnen gelöschten Datensatz handelt.

Es werden nicht mehrere Auditlog-Einträge für die einzelnen Seiten einer Verschiebung oder für jede Position einer Sammelentnahme erzeugt. Die komplette Aktion wird in einem Eintrag zusammengehalten.

## Technischer Tabellenentwurf

Die endgültigen Namen und Typen werden in der EF-Core-Konfiguration festgelegt. Inhaltlich werden mindestens folgende Informationen benötigt:

| Information | Zweck |
| --- | --- |
| Eindeutige ID | Identifiziert den Auditlog-Eintrag und damit die Aktion |
| Zeitpunkt (UTC) | Zeitpunkt der erfolgreich gespeicherten Änderung |
| Benutzer-ID | Zuordnung zum auslösenden Benutzer |
| Benutzername-Snapshot | Lesbare Zuordnung auch dann, wenn sich der Benutzername später ändert |
| Ereignistyp | Art der Aktion, z. B. Artikeländerung, Zugang, Verschiebung oder Sammelentnahme |
| Vorher-Zustand (JSONB) | Vollständiger Snapshot aller betroffenen Daten vor der Aktion |
| Nachher-Zustand (JSONB) | Vollständiger Snapshot aller betroffenen Daten nach der Aktion |

Es wird keine einzelne `EntityId` als einzige Zielreferenz vorausgesetzt, weil eine Aktion mehrere Datensätze betreffen kann. Die betroffenen Datensatz-IDs sind Bestandteil der Snapshots.

Die Snapshots enthalten ausschließlich die vollständigen fachlichen Zustände und IDs, keine EF-Navigationseigenschaften oder zyklischen Objektgraphen. Der Vorher- und Nachher-Zustand wird als zusammengehöriger Snapshot der gesamten Aktion gespeichert.

## Schutz und Konsistenz

- Die Anwendung legt Auditlog-Einträge an; im normalen Betrieb werden bestehende Einträge weder geändert noch gelöscht.
- Es gibt keine Fremdschlüssel-Kaskade vom Auditlog zu aktuellen Stammdaten oder Materialposten.
- Jede schreibende fachliche Aktion muss den Auditlog-Eintrag in derselben Transaktion speichern. Scheitert das Protokollieren, wird auch die fachliche Änderung zurückgerollt.
- Die Historienansicht für Materialbewegungen liest ausschließlich aus dieser Auditlog-Tabelle und filtert nach den entsprechenden Ereignistypen.
- Ein technischer Schreibdienst darf verwendet werden; er schreibt ausschließlich in dieselbe zentrale Auditlog-Tabelle und bildet kein zweites Protokoll.

## Noch auszuarbeitende Implementierungsdetails

Die konkreten Ereignistypen, JSON-Feldnamen, Indizes und EF-Core-Konfiguration sind technische Ausgestaltung dieses bereits festgelegten Modells. Sie ändern nichts an den verbindlichen Punkten: **eine Tabelle, ein Eintrag pro Aktion, vollständiger Vorher-/Nachher-Zustand, auslösender Benutzer.**
