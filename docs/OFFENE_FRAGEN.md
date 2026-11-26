# Offene fachliche und technische Entscheidungen

Diese Punkte sind bewusst offen. Wo ein sicherer technischer Ausgangsstandard bereits implementiert ist, wird er bis zur abschließenden Produktentscheidung nicht stillschweigend gelockert.

## Fachlich

1. **Standardstandort:** Kein Standort wird automatisch angelegt und es gibt keinen fest eingebauten Standort namens „Lager“. Eine spätere explizite Standard-Quelle bzw. ein Standard-Ziel ist nicht beschlossen.
2. **Merge-Regel bei fehlenden Chargen:** Implementierter sicherer Ausgangsstand: NULL-Charge und bekannte Charge gelten als unterschiedliche Identität und werden nicht zusammengeführt. Fachlich final bestätigen, bevor eine weniger strenge Regel erwogen wird.
3. **Hersteller als Identitätsmerkmal:** Implementierter sicherer Ausgangsstand: Hersteller muss exakt übereinstimmen (einschließlich NULL), damit Materialposten zusammengeführt werden. So wird kein Herstellerwert willkürlich überschrieben. Fachlich final bestätigen.
4. **Sammelentnahme-Zuordnung:** Implementiert ist die Auswahl konkreter Materialposten per UUID; es findet keine automatische Verteilung über mehrere Chargen/Ablaufdaten (FIFO/FEFO) statt. Diese klare Zuordnung sollte für Version 1.0 bestätigt werden.
5. **Löschen von Artikeln/Standorten:** Implementiert ist: Löschen nur ohne aktuelle Materialposten. Historieneinträge blockieren nicht, da AuditLog vollständige Snapshots ohne FK-Verknüpfung enthält. Vor Produktivstart fachlich bestätigen, ob diese Regel genügt oder Archivierung gewünscht ist.
6. **AuditLog:** Verbindlich: genau eine zentrale AuditLog-Tabelle, ein Eintrag je erfolgreich abgeschlossener Aktion, vollständiger Vorher-/Nachher-Zustand aller betroffenen Daten und Zuordnung zum auslösenden Benutzer. Es gibt keine `StockMovement`-Tabelle und keine separate Bewegungshistorie.
7. **Kontoverwaltung:** E-Mail-Adresse und Passwort über Supabase Auth sind beschlossen; öffentliche Selbstregistrierung ist deaktiviert. Profile erhalten zunächst keine Anwendungsrolle. Bootstrap des ersten Administrators ist dokumentiert; Admin-Einladungsfunktion sowie Passwortänderung/-zurücksetzung sind noch umzusetzen.

## Technisch

8. **Frontend-Hosting:** Hosting-Anbieter und Deployment-Umgebung für TanStack Start auswählen; Supabase Cloud ist als Datenbank-/Auth-Host beschlossen.
9. **Supabase Cloud Region:** Frankfurt (EU Central) ist bevorzugt, aber das Cloud-Projekt ist noch nicht provisioniert.
10. **Pagination und Fehlerbehandlung:** Einheitliche Query-Pagination und Übersetzung von PostgreSQL-/PostgREST-Fehlern in UI-Meldungen vor der Inventaroberfläche abschließen.
11. **Betrieb und Backup:** Backup-Tarif, Aufbewahrung und regelmäßigen Restore-Test vor produktivem Materialbetrieb festlegen.

## Bereits entschieden – nicht erneut als offen behandeln

- Mengeneinheit: Stück, ganze Zahlen.
- Keine automatische Standortanlage.
- Keine Standorttypen.
- Vollständig entnommene Materialposten werden gelöscht.
- Historische Informationen bleiben im zentralen AuditLog erhalten.
- Genau ein AuditLog für alle Anwendungsbereiche und keine separate Materialbewegungstabelle.
- Artikel ohne Ablaufdatum werden unterstützt.
- Kernvorgänge: Zugang, Verschiebung, Sammelentnahme.
- Supabase Auth mit E-Mail-Adresse und Passwort; keine öffentliche Selbstregistrierung.
- Neue Profile haben zunächst keine Anwendungsrolle; Administratorfreigabe erfolgt kontrolliert.
- Direkte Tabellenänderungen sind gesperrt; CRUD- und Bestandsänderungen laufen über auditiertes RPC.
- Optionale Funktionen dürfen das Kernsystem nicht blockieren.
