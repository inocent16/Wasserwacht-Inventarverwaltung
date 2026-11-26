# Wasserwacht Inventarverwaltung

Modulare Inventar- und Materialverwaltung für eine Wasserwacht-Ortsgruppe.

## Ziel

Eine Webanwendung zur Verwaltung von Artikeln, Standorten, aktuellen Materialposten und Materialaktionen. Datenbank und Authentifizierung werden zunächst über Supabase Cloud bereitgestellt.

## Zieltechnologie
- Frontend und Fullstack: SolidJS 1.9 + TanStack Start
- Routing: TanStack Router
- Server-State: TanStack Query
- Datenbank: Supabase Cloud / PostgreSQL
- Authentifizierung: Supabase Auth
- SSR-Session-Integration: `@supabase/ssr`
- Validierung: TypeScript + Zod
- SQL-Migrationen und Tests: Supabase CLI + pgTAP

Die Versionsentscheidung ist in [docs/STACK-MIGRATION.md](docs/STACK-MIGRATION.md) beschrieben. Das bisherige ASP.NET-Core-/EF-Core-Backend bleibt während der Migration als Referenz erhalten.

## Kernumfang Version 1.0
- Artikelverwaltung
- Standortverwaltung
- Materialposten je Artikel und Standort
- Materialzugang
- Verschiebung zwischen Standorten
- Sammelentnahme mehrerer Artikel aus einem Standort
- zentrale Änderungs- und Bewegungshistorie (ein zentrales AuditLog)
- persönliche Konten mit E-Mail-Adresse und Passwort; zum Start ausschließlich Administrator

## Verbindliche fachliche Regeln
- Alle Artikelmengen werden in **Stück** als ganze Zahlen geführt.
- Alle Standorte sind gleichberechtigt; es gibt keine Standorttypen.
- Standorte werden durch Anwender angelegt. Beim ersten Start wird keiner automatisch erzeugt.
- Es gibt keine fest eingebaute Sonderrolle oder Standardstandort-Logik für „Lager“.
- Ablaufdatum, Charge und Hersteller sind bei Materialposten optional.
- Artikel ohne Ablaufdatum werden unterstützt.
- Passende Materialposten werden bei Zugang und Verschiebung nach exakter Identität zusammengeführt (Artikel, Standort, Ablaufdatum, Charge und Hersteller).
- Fehlende Chargen-/Herstellerwerte werden nicht stillschweigend mit bekannten Werten vermischt.
- Bei vollständiger Entnahme wird der Materialposten gelöscht. Die zugehörige Historie bleibt dauerhaft erhalten.
- Genau eine zentrale AuditLog-Tabelle ist die einzige Quelle der Änderungshistorie.
- Jede erfolgreiche fachliche Aktion erzeugt genau einen AuditLog-Eintrag mit vollständigem Vorher- und Nachher-Zustand aller betroffenen Daten.
- Die gesamte Bestandsänderung und dieser eine AuditLog-Eintrag werden gemeinsam transaktional gespeichert.
- Es gibt keine `StockMovement`-Tabelle und keine separate Bewegungshistorie.
- Zum Start gibt es ausschließlich die Rolle Administrator; öffentliche Selbstregistrierung ist deaktiviert.
- Optionale Funktionen dürfen keine Voraussetzung für den Kernbetrieb sein.

## Entwicklerdokumentation
- [Projektstatus und Übergabe](docs/PROJEKTSTATUS.md)
- [Anforderungen](docs/ANFORDERUNGEN.md)
- [Technische und fachliche Entscheidungen](docs/ENTSCHEIDUNGEN.md)
- [Offene Fragen](docs/OFFENE_FRAGEN.md)
- [Architektur](docs/ARCHITEKTUR.md)
- [Stack-Migration](docs/STACK-MIGRATION.md)
- [AuditLog-Konzept](docs/AUDITLOG-KONZEPT.md)
- [Fachliches Datenmodell](docs/DOMAENENMODELL.md)
- [API-/RPC-Kontrakt](docs/API-KONTRAKT.md)
- [Entwicklerstart](docs/ENTWICKLERSTART.md)
- [Roadmap](docs/ROADMAP.md)

## Aktueller Stand
Der Migrationsbranch enthält das Solid-/TanStack-Grundgerüst, Supabase-Auth-Login, SQL-Schema, RLS, auditiertes CRUD-RPC und transaktionale Bestandsfunktionen. Cloud-Projekt und Credentials sind noch nicht eingerichtet. Die Datenbankmigrationen und pgTAP-Tests müssen in CI bzw. gegen die lokale Supabase-Umgebung verifiziert werden. Details stehen in [docs/PROJEKTSTATUS.md](docs/PROJEKTSTATUS.md).
