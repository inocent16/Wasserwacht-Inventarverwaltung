# Mitwirken

## Vor einer Implementierung

1. Anforderungen und Entscheidungen in `docs/` lesen.
2. `docs/OFFENE_FRAGEN.md` prüfen. Offene fachliche Fragen nicht eigenmächtig entscheiden.
3. Änderungen klein und nachvollziehbar halten.
4. Geschäftsregeln im Backend implementieren; das Frontend ist kein Ort für maßgebliche Bestandslogik.

## Qualitätsregeln

- Jede Bestandsänderung läuft über einen Anwendungsservice.
- Bestandsänderung und Historieneintrag müssen in derselben Datenbanktransaktion liegen.
- Ein vollständig entnommener Materialposten wird gelöscht; Historieneinträge bleiben unabhängig bestehen.
- Mengen sind positive Ganzzahlen in Stück.
- Keine automatische Anlage eines Standorts.
- Keine Sonderbehandlung des Standorts „Lager“ ohne neue fachliche Entscheidung.
- Neue fachliche Regeln benötigen automatisierte Tests.
- Keine Secrets, echten Passwörter oder personenbezogenen Echtdaten committen.

## Pull Requests

Beschreiben:
- fachliche Änderung
- technische Umsetzung
- Datenbankmigrationen (falls vorhanden)
- Tests und tatsächliches Testergebnis
- verbleibende offene Punkte
