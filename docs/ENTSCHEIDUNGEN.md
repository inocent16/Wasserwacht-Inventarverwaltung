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
