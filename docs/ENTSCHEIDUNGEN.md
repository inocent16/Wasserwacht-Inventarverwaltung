# Technische und fachliche Entscheidungen

## Mengeneinheit

Alle Artikel werden zunächst ausschließlich in **Stück** geführt. Mengen werden als ganze Zahlen gespeichert.

## Kategorien

In der ersten Ausbaustufe ist beides möglich:
- Kategorien können beim Artikel frei eingegeben werden.
- Bereits verwendete Kategorien sollen als Auswahl bzw. Vorschlag verfügbar sein.

Die Kategorie wird zunächst als Text am Artikel gespeichert. Später wird auf eine verwaltbare Kategorienliste mit eigenen Kategorie-Datensätzen umgestellt. Bei dieser Umstellung müssen vorhandene Kategorien übernommen werden; gleiche Schreibweisen sollen zusammengeführt werden können.

## Datenmodell – erster Stand

Das Backend-Grundmodell enthält Artikel, Standorte und Materialposten. Für die gesamte Anwendung gibt es genau eine zentrale Auditlog-Tabelle als einzige Quelle der Änderungshistorie. Es gibt keine `StockMovement`-Tabelle, keine separate Bewegungshistorie und kein zweites Protokollsystem. Jede erfolgreich abgeschlossene fachliche Aktion erzeugt genau einen Auditlog-Eintrag. Auch Bestandszugänge, Verschiebungen und Sammelentnahmen werden jeweils als eine Aktion in diesem zentralen Auditlog protokolliert. Jeder Eintrag speichert – soweit für den jeweiligen Ereignistyp anwendbar – den vollständigen Zustand aller betroffenen Daten vor und nach der Aktion sowie die Identität des Benutzers, der die Änderung ausgelöst hat. Die fachliche Änderung und der eine dazugehörige Auditlog-Eintrag werden gemeinsam transaktional gespeichert.

## Benutzeranmeldung

Version 1.0 besitzt eine Benutzeranmeldung mit persönlichen Benutzerkonten. Zum Start gibt es ausschließlich die Rolle `Administrator`; eine zusätzliche Standardbenutzerrolle oder feinere Berechtigungsstufen sind zunächst nicht vorgesehen. Jedes angelegte Konto erhält diese Rolle. Benutzerkonten werden ausschließlich durch einen Administrator angelegt; der Administrator stellt den neuen Benutzern ihre Zugangsdaten bereit. Auditlog-Einträge ordnen jede Änderung dem angemeldeten Benutzer zu. Das technische Anmeldeverfahren und die sichere erstmalige Einrichtung des ersten Administratorkontos werden vor der Implementierung festgelegt.

## Historie / Auditlog

- Das zentrale Auditlog ist verbindlicher Bestandteil des Kernsystems und protokolliert Änderungen in allen Anwendungsbereichen.
- Es gibt keine zusätzliche, separate Materialbewegungstabelle.
- Die Bewegungshistorie wird aus den passenden Auditlog-Einträgen dargestellt.
- Audit-Einträge bleiben unabhängig vom Lebenszyklus der betroffenen Stammdaten und Materialposten erhalten; keine Kaskadenlöschung historischer Daten.

## Initialer Standort

Es wird beim ersten Start kein Standort automatisch angelegt. Standorte werden durch die Anwender selbst erstellt. Es gibt keinen technisch fest vorgegebenen Standort namens „Lager“.

## Materialposten bei vollständiger Entnahme

Wird die Menge eines Materialpostens vollständig entnommen, wird der Materialposten gelöscht, statt mit Menge 0 bestehen zu bleiben. Die dazugehörigen Auditlog-Einträge bleiben unabhängig davon dauerhaft erhalten. Die Löschung des aktuellen Bestandsdatensatzes darf daher niemals historische Auditdaten löschen.
