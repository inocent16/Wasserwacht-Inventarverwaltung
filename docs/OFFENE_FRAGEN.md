# Offene fachliche und technische Entscheidungen

Diese Punkte sind bewusst offen. Entwickler sollen keine stillschweigenden Produktentscheidungen treffen. Vor der jeweils betroffenen Implementierung Rücksprache halten.

## Fachlich

1. **Standardstandort:** Es wurde bestätigt, dass kein Standort automatisch angelegt wird und kein fest eingebauter Standort namens „Lager“ existiert. Soll es später eine vom Benutzer explizit auswählbare Standard-Quelle bzw. Standard-Ziel geben? Bis dahin keine Standardstandort-Logik implementieren.
2. **Merge-Regel bei fehlenden Chargen:** Wenn ein vorhandener Posten keine Charge hat und ein Zugang eine Charge angibt (oder umgekehrt), dürfen diese zusammengeführt werden oder müssen sie getrennt bleiben?
3. **Hersteller als Identitätsmerkmal:** Muss der Hersteller beim Zusammenführen identisch sein, oder ist er nur ergänzende Information? Bei Konflikt (ein Wert fehlt / Werte weichen ab) darf nicht willkürlich überschrieben werden.
4. **Sammelentnahme-Zuordnung:** Soll eine Entnahmeposition einen konkreten Materialposten auswählen (damit Ablaufdatum/Charge eindeutig sind), oder darf die Anwendung eine Menge automatisch über mehrere passende Posten verteilen? Falls automatisch: nach welcher Reihenfolge (z. B. frühestes Ablaufdatum zuerst)?
5. **Löschen von Artikeln/Standorten:** Welche Regeln gelten bei vorhandenen Beständen und historischen Referenzen? Technisch ist Soft-Delete/Archivierung eine Option, aber noch nicht beschlossen.
6. **Technische Auditlog-Ausgestaltung:** Die fachliche Entscheidung ist verbindlich und nicht mehr offen: genau eine zentrale Auditlog-Tabelle, ein Eintrag je erfolgreich abgeschlossener Aktion, vollständiger Vorher- und Nachher-Zustand aller betroffenen Daten sowie Zuordnung zum auslösenden Benutzer. Es gibt keine `StockMovement`-Tabelle und keine separate Bewegungshistorie. Die konkreten technischen Ereignistypen, JSON-Feldnamen, Indizes und EF-Core-Konfiguration werden bei der Implementierung gemäß `AUDITLOG-KONZEPT.md` festgelegt; hierfür ist keine erneute Grundsatzentscheidung nötig.
7. **Technische Ausgestaltung der Anmeldung:** Die Anmeldung mit persönlichen Konten ist für Version 1.0 beschlossen. Zum Start gibt es ausschließlich die Rolle `Administrator`; jedes angelegte Konto erhält diese Rolle. Konten werden durch einen angemeldeten Administrator angelegt. Noch festzulegen sind das konkrete Authentifizierungsverfahren, die sichere erstmalige Einrichtung des ersten Administratorkontos sowie der Ablauf für Passwortänderung und Passwortzurücksetzung.

## Technisch

8. **Deployment lokal:** Zielbetriebssystem und bevorzugte Bereitstellung (Docker Compose oder native Installation) festlegen.
9. **Frontend-Toolchain:** Vite ist naheliegend, aber noch nicht ausdrücklich beschlossen.
10. **API-Fehlerformat und Pagination:** Vor Erstellung der produktiven API verbindlich definieren.

## Bereits entschieden – nicht erneut als offen behandeln

- Mengeneinheit: Stück, ganze Zahlen.
- Keine automatische Standortanlage.
- Keine Standorttypen.
- Vollständig entnommene Materialposten werden gelöscht.
- Historische Informationen bleiben im zentralen Auditlog erhalten.
- Es gibt genau ein Auditlog für alle Anwendungsbereiche und keine separate Materialbewegungstabelle.
- Artikel ohne Ablaufdatum werden unterstützt.
- Kernvorgänge: Zugang, Verschiebung, Sammelentnahme.
- Passende Posten werden bei Zugang und Verschiebung zusammengeführt.
- Optionale Funktionen dürfen das Kernsystem nicht blockieren.
