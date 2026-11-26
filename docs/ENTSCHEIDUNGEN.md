# Technische und fachliche Entscheidungen

## Mengeneinheit

Alle Artikel werden zunächst ausschließlich in **Stück** geführt. Mengen werden als ganze Zahlen gespeichert.

## Kategorien

In der ersten Ausbaustufe ist beides möglich:
- Kategorien können beim Artikel frei eingegeben werden.
- Bereits verwendete Kategorien sollen als Auswahl bzw. Vorschlag verfügbar sein.

Die Kategorie wird zunächst als Text am Artikel gespeichert. Später wird auf eine verwaltbare Kategorienliste mit eigenen Kategorie-Datensätzen umgestellt. Bei dieser Umstellung müssen vorhandene Kategorien übernommen werden; gleiche Schreibweisen sollen zusammengeführt werden können.

## Datenmodell – erster Stand

Das Backend-Grundmodell enthält Artikel, Standorte, Materialposten und Materialbewegungen. Optionale Erweiterungen werden erst nach dem Kern umgesetzt.

## Initialer Standort

Es wird beim ersten Start kein Standort automatisch angelegt. Standorte werden durch die Anwender selbst erstellt. Es gibt keinen technisch fest vorgegebenen Standort namens „Lager“.

## Materialposten bei vollständiger Entnahme

Wird die Menge eines Materialpostens vollständig entnommen, wird der Materialposten gelöscht, statt mit Menge 0 bestehen zu bleiben. Die dazugehörige Bewegungshistorie bleibt unabhängig davon dauerhaft erhalten. Die Löschung des aktuellen Bestandsdatensatzes darf daher niemals die historischen Bewegungsdaten löschen.
