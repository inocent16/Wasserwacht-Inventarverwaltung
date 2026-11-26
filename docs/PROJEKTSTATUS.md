# Projektstatus und Entwicklerübergabe

Stand: 26.11.2026  
Ziel: Version 1.0

## Kurzüberblick

Es entsteht eine lokale Webanwendung zur Verwaltung des Materials einer Wasserwacht-Ortsgruppe. Der verbindliche Kern besteht aus Artikelverwaltung, Standortverwaltung, Materialposten und Materialbewegungen. Optionale Funktionen dürfen diesen Kern nicht voraussetzen oder blockieren.

## Aktueller technischer Stand

Vorhanden:
- ASP.NET-Core-Webprojekt unter `backend/src/Wasserwacht.Api`
- Entity-Framework-Core-/Npgsql-Abhängigkeiten
- erste Domainklassen für Artikel, Standorte, Materialposten und Bewegungsdatensätze
- EF-Core-DbContext und einfacher Health-Endpunkt
- grundlegende Anforderungen und Entscheidungen in `docs/`

Noch nicht vorhanden:
- Datenbankmigrationen
- fachliche Services und Validierungen
- CRUD- und Bewegungsendpunkte
- Authentifizierung und Benutzerverwaltung (für Version 1.0 verpflichtend; zunächst ausschließlich Rolle Administrator)
- zentrales Auditlog als implementierte Historienquelle
- React-Frontend
- automatisierte Tests
- Docker-/lokale Startkonfiguration

**Wichtig:** Der vorhandene Code ist ein technisches Grundgerüst, keine lauffähige Produktversion. Ein erfolgreicher Build oder Integrationstest ist bisher nicht dokumentiert.

## Verbindlicher Produktumfang

1. Artikel anlegen, anzeigen, ändern und löschen (Löschregeln noch fachlich festzulegen).
2. Standorte anlegen, anzeigen, ändern und löschen (Löschregeln noch fachlich festzulegen).
3. Materialposten je Artikel und Standort verwalten.
4. Materialzugänge erfassen.
5. Material zwischen Standorten verschieben.
6. Mehrere Artikel in einer Sammelentnahme aus einem Standort ausbuchen.
7. Historische Bewegungsinformationen bleiben erhalten, auch wenn ein aktueller Materialposten vollständig aufgebraucht und gelöscht wird.

Alle Mengen sind ganze Zahlen in der Einheit Stück.

## Leitplanken für die Umsetzung

- Keine automatische Anlage eines Standorts beim ersten Start.
- Keine fest eingebauten Standorttypen.
- Kein fest eingebauter Standort namens „Lager“ und keine implizite Standardstandort-Logik, solange diese nicht ausdrücklich entschieden ist.
- Artikel ohne Ablaufdatum müssen unterstützt werden.
- Identische Bestände werden bei Zugang und Verschiebung zusammengeführt.
- Unterschiedliche bekannte Chargennummern bleiben getrennt.
- Vollständig entnommene Materialposten werden gelöscht; historische Bewegungsinformationen bleiben erhalten.
- Mehrbenutzerfähigkeit von Anfang an architektonisch berücksichtigen.
- Anmeldung und Autorisierung bereits vor den fachlichen Schreibendpunkten einplanen; zum Start ausschließlich Rolle Administrator.
- Das zentrale Auditlog ist Pflichtbestandteil und einzige Historienquelle, keine optionale Erweiterung.
- Optionale Funktionen modular und unabhängig halten.

## Reihenfolge

Siehe `ROADMAP.md`. Die erste technische Lieferung soll ein durch Tests abgesichertes Backend-Fundament einschließlich Authentifizierung, Administratorkonto, zentralem Auditlog und anschließender Stammdaten-/Bestandslogik sein. Danach folgt die Weboberfläche.
