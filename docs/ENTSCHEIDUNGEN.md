# Technische und fachliche Entscheidungen

## Mengeneinheit

Alle Artikel werden zunächst ausschließlich in **Stück** geführt. Mengen werden als ganze Zahlen gespeichert.

## Kategorien

In der ersten Ausbaustufe ist beides möglich:
- Kategorien können beim Artikel frei eingegeben werden.
- Bereits verwendete Kategorien sollen als Auswahl bzw. Vorschlag verfügbar sein.

Die Kategorie wird zunächst als Text am Artikel gespeichert. Später wird auf eine verwaltbare Kategorienliste mit eigenen Kategorie-Datensätzen umgestellt. Bei dieser Umstellung müssen vorhandene Kategorien übernommen werden; gleiche Schreibweisen sollen zusammengeführt werden können.

## Datenmodell – erster Stand

Das Backend-Grundmodell enthält Artikel, Standorte und Materialposten. Für die gesamte Anwendung gibt es genau ein zentrales Auditlog als einzige Quelle der Änderungshistorie. Eine separate Materialbewegungstabelle wird nicht verwendet. Auch Bestandszugänge, Verschiebungen und Entnahmen werden als Auditlog-Einträge protokolliert. Jeder Auditlog-Eintrag speichert – soweit für den jeweiligen Ereignistyp anwendbar – den vollständigen Zustand vor und nach der Änderung sowie die Identität des Benutzers, der die Änderung ausgelöst hat. Das konkrete Schema und die Referenzierungsstrategie werden vor der Implementierung festgelegt.

## Benutzeranmeldung

Version 1.0 besitzt eine Benutzeranmeldung mit persönlichen Benutzerkonten. Auditlog-Einträge ordnen jede Änderung dem angemeldeten Benutzer zu. Das Verfahren zur Anlage und Verwaltung lokaler Benutzerkonten ist noch festzulegen.

## Historie / Auditlog

- Ein zentrales Auditlog protokolliert Änderungen in allen Anwendungsbereichen.
- Es gibt keine zusätzliche, separate Materialbewegungstabelle.
- Die Bewegungshistorie wird aus den passenden Auditlog-Einträgen dargestellt.
- Audit-Einträge bleiben unabhängig vom Lebenszyklus der betroffenen Stammdaten und Materialposten erhalten; keine Kaskadenlöschung historischer Daten.

## Initialer Standort

Es wird beim ersten Start kein Standort automatisch angelegt. Standorte werden durch die Anwender selbst erstellt. Es gibt keinen technisch fest vorgegebenen Standort namens „Lager“.

## Materialposten bei vollständiger Entnahme

Wird die Menge eines Materialpostens vollständig entnommen, wird der Materialposten gelöscht, statt mit Menge 0 bestehen zu bleiben. Die dazugehörigen Auditlog-Einträge bleiben unabhängig davon dauerhaft erhalten. Die Löschung des aktuellen Bestandsdatensatzes darf daher niemals historische Auditdaten löschen.
