# Wasserwacht Inventarverwaltung

Modulare Inventar- und Materialverwaltung für eine Wasserwacht-Ortsgruppe.

## Projektziel

Eine lokal betreibbare Webanwendung zur Verwaltung von Artikeln, Standorten, aktuellen Materialposten und Materialbewegungen. Das Pflichtsystem besteht ausschließlich aus diesen Kernbereichen. Optionale Funktionen werden unabhängig ergänzt und dürfen die Kernverwaltung nicht blockieren.

## Technologie

- Frontend: React + TypeScript
- Backend: ASP.NET Core / C#
- Datenbank: PostgreSQL
- ORM: Entity Framework Core / Npgsql
- zunächst lokaler Betrieb; Mehrbenutzerfähigkeit von Anfang an berücksichtigen
- mobile Anwendung erst nach Fertigstellung der Webanwendung, dann mit gleichem Funktionsumfang

## Kernumfang Version 1.0

- Artikelverwaltung
- Standortverwaltung
- Materialposten je Artikel und Standort
- Materialzugang
- Verschiebung zwischen Standorten
- Sammelentnahme mehrerer Artikel aus einem Standort
- zentrale Änderungs- und Bewegungshistorie (Auditlog)
- Anmeldung mit persönlichen Konten; zum Start ausschließlich die Rolle Administrator

## Verbindliche fachliche Regeln

- Alle Artikelmengen werden in **Stück** als ganze Zahlen geführt.
- Alle Standorte sind gleichberechtigt; es gibt keine Standorttypen.
- Standorte werden durch Anwender angelegt. Beim ersten Start wird keiner automatisch erzeugt.
- Es gibt keine fest eingebaute Sonderrolle oder Standardstandort-Logik für „Lager“.
- Ablaufdatum, Charge und Hersteller sind bei Materialposten optional.
- Artikel ohne Ablaufdatum werden unterstützt.
- Passende Materialposten werden sowohl bei Zugang als auch bei Verschiebung zusammengeführt.
- Unterschiedliche bekannte Chargennummern bleiben getrennt.
- Bei vollständiger Entnahme wird der Materialposten gelöscht. Die zugehörige Historie bleibt dauerhaft erhalten.
- Ein zentrales Auditlog ist die einzige Quelle der Änderungshistorie und gehört zum Pflichtumfang.
- Bestandsänderungen und zugehörige Auditlog-Einträge werden transaktional gespeichert.
- Zum Start gibt es ausschließlich die Rolle Administrator; Konten werden durch einen Administrator angelegt.
- Optionale Funktionen dürfen keine Voraussetzung für den Kernbetrieb sein.

## Optionale Erweiterungen

Mögliche unabhängig nutzbare Erweiterungen sind Sollbestände und Soll-Ist-Warnungen, Ablaufwarnungen, Materialtausch, Verplombung und Kontrollen pro Standort, Nachfüllhinweise, Bestellkennzeichnung sowie Backup und Wiederherstellung. Sie sind nicht Voraussetzung für den Kernumfang.

## Dokumentation für Entwickler

- [Projektstatus und Übergabe](docs/PROJEKTSTATUS.md)
- [Anforderungen](docs/ANFORDERUNGEN.md)
- [Entscheidungen](docs/ENTSCHEIDUNGEN.md)
- [Offene Fragen – vor betroffenen Implementierungen klären](docs/OFFENE_FRAGEN.md)
- [Architektur](docs/ARCHITEKTUR.md)
- [Fachliches Datenmodell](docs/DOMAENENMODELL.md)
- [API-Kontrakt (Entwurf)](docs/API-KONTRAKT.md)
- [Entwicklerstart](docs/ENTWICKLERSTART.md)
- [Roadmap](docs/ROADMAP.md)

## Aktueller Entwicklungsstand

Das Repository enthält ein erstes ASP.NET-Core-Grundgerüst und die fachliche/technische Entwicklerdokumentation. Es gibt noch keine vollständige API, keine Datenbankmigrationen, keine React-Anwendung und keine automatisierten Tests. Der Status ist in [docs/PROJEKTSTATUS.md](docs/PROJEKTSTATUS.md) detailliert beschrieben.
