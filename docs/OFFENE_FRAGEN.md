# Offene fachliche und technische Entscheidungen

Diese Punkte sind bewusst offen. Entwickler sollen keine stillschweigenden Produktentscheidungen treffen. Vor der jeweils betroffenen Implementierung Rücksprache halten.

## Fachlich

1. **Historienmodell:** Die Projektübergabe legt ein zentrales Auditlog fest und sagt, eine separate Materialbewegungstabelle sei nicht vorgesehen. Der aktuelle Code enthält bereits `StockMovement`. Soll die Bewegungsübersicht aus einem zentralen Auditlog gespeist werden oder soll es zusätzlich eine eigene, strukturierte Bewegungstabelle geben? Die Information, dass die Historie erhalten bleibt, ist bestätigt; die technische Form ist noch nicht abschließend geklärt.
2. **Standardstandort:** Es wurde bestätigt, dass kein Standort automatisch angelegt wird und kein fest eingebauter Standort namens „Lager“ existiert. Soll es später eine vom Benutzer explizit auswählbare Standard-Quelle bzw. Standard-Ziel geben? Bis dahin keine Standardstandort-Logik implementieren.
3. **Merge-Regel bei fehlenden Chargen:** Wenn ein vorhandener Posten keine Charge hat und ein Zugang eine Charge angibt (oder umgekehrt), dürfen diese zusammengeführt werden oder müssen sie getrennt bleiben?
4. **Hersteller als Identitätsmerkmal:** Muss der Hersteller beim Zusammenführen identisch sein, oder ist er nur ergänzende Information? Bei Konflikt (ein Wert fehlt / Werte weichen ab) darf nicht willkürlich überschrieben werden.
5. **Sammelentnahme-Zuordnung:** Soll eine Entnahmeposition einen konkreten Materialposten auswählen (damit Ablaufdatum/Charge eindeutig sind), oder darf die Anwendung eine Menge automatisch über mehrere passende Posten verteilen? Falls automatisch: nach welcher Reihenfolge (z. B. frühestes Ablaufdatum zuerst)?
6. **Löschen von Artikeln/Standorten:** Welche Regeln gelten bei vorhandenen Beständen und historischen Referenzen? Technisch ist Soft-Delete/Archivierung eine Option, aber noch nicht beschlossen.
7. **Bewegungsdaten:** Welche Pflichtangaben gelten für Notiz/Grund, Benutzer und Zeitpunkt? Zeitpunkt kann serverseitig erfasst werden; Benutzeridentität hängt von der noch festzulegenden Authentifizierung ab.
8. **Benutzeranmeldung für Version 1.0:** Die Übergabe nennt zunächst eine Admin-Rolle und spätere Rollen. Soll bereits die erste lokale Version einen Login haben, und wie werden lokale Benutzer angelegt?

## Technisch

9. **Deployment lokal:** Zielbetriebssystem und bevorzugte Bereitstellung (Docker Compose oder native Installation) festlegen.
10. **Frontend-Toolchain:** Vite ist naheliegend, aber noch nicht ausdrücklich beschlossen.
11. **API-Fehlerformat und Pagination:** Vor Erstellung der produktiven API verbindlich definieren.

## Bereits entschieden – nicht erneut als offen behandeln

- Mengeneinheit: Stück, ganze Zahlen.
- Keine automatische Standortanlage.
- Keine Standorttypen.
- Vollständig entnommene Materialposten werden gelöscht.
- Historische Bewegungsinformationen bleiben erhalten.
- Artikel ohne Ablaufdatum werden unterstützt.
- Kernvorgänge: Zugang, Verschiebung, Sammelentnahme.
- Passende Posten werden bei Zugang und Verschiebung zusammengeführt.
- Optionale Funktionen dürfen das Kernsystem nicht blockieren.
