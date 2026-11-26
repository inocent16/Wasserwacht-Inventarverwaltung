# Projektstatus und Entwicklerübergabe

Stand: 26.11.2026
Ziel: Version 1.0

## Kurzüberblick
Es entsteht eine Webanwendung zur Verwaltung des Materials einer Wasserwacht-Ortsgruppe. Die Zielarchitektur wurde auf SolidJS + TanStack Start + Supabase Cloud umgestellt. Das bisherige C#-Backend bleibt während der Migration als Referenz erhalten.

## Zieltechnologie
- SolidJS 1.9
- TanStack Start 1.x / TanStack Router
- TanStack Query 5.x
- Supabase Cloud: PostgreSQL und Auth
- `@supabase/ssr` für Cookie-basierte SSR-Sitzungen
- TypeScript und Zod

Die stabile TanStack-Solid-Start-Version verwendet derzeit Solid 1.x. Solid 2 bleibt ein späteres Upgrade, sobald die zugehörige TanStack-Integration stabil ist.

## Bereits vorhanden

### Bisheriger Stand (Übergangsbestand)
- ASP.NET-Core-Webprojekt unter `backend/src/Wasserwacht.Api`
- EF-Core-/Npgsql-Abhängigkeiten und PostgreSQL-Migrationen
- Domainklassen für Artikel, Standorte, Materialposten und AuditLog
- Administrator-Anmeldung im C#-Backend
- Tests und getrennte Backend-CI

### Neue Plattformbasis auf dem Migrationsbranch
- React-Abhängigkeiten durch SolidJS ersetzt
- TanStack Start und TanStack Router als Fullstack-/Routing-Grundlage
- TanStack Query Provider und request-scoped QueryClient vorbereitet
- Supabase Browser- und Server-Client mit `@supabase/ssr` vorbereitet
- Supabase-Umgebungsvariablen dokumentiert
- Architektur- und Entwicklerdokumentation auf die Zielplattform umgestellt

## Noch nicht vorhanden
- Supabase-Cloud-Projekt und konfigurierte Secrets
- SQL-Migrationen für das neue Schema
- RLS-Policies und Sicherheitstests
- Supabase-Auth-Flows und initiale Administratoranlage
- fachliche CRUD-Funktionen für Artikel und Standorte
- Bestandsabfragen sowie Zugang, Verschiebung und Sammelentnahme
- transaktionale PostgreSQL-Funktionen und zentrale AuditLog-Implementierung
- vollständige Inventaroberfläche
- Deployment-Konfiguration und End-to-End-Tests

## Verbindlicher Produktumfang
1. Artikelverwaltung.
2. Standortverwaltung.
3. Materialposten je Artikel und Standort.
4. Materialzugänge.
5. Verschiebung zwischen Standorten.
6. Sammelentnahme mehrerer Artikel aus einem Standort.
7. Dauerhafte Historie auch nach vollständiger Entnahme eines Materialpostens.

Alle Mengen sind ganze Zahlen in Stück.

## Leitplanken
- Keine automatische Standortanlage.
- Keine Standorttypen und kein fest eingebauter Standort „Lager“.
- Artikel ohne Ablaufdatum werden unterstützt.
- Passende Posten werden bei Zugang und Verschiebung zusammengeführt.
- Unterschiedliche bekannte Chargennummern bleiben getrennt.
- Vollständig entnommene Posten werden gelöscht, Historieneinträge bleiben.
- Genau eine zentrale AuditLog-Tabelle, genau ein Eintrag pro erfolgreicher fachlicher Aktion.
- Es gibt keine StockMovement-Tabelle und keine separate Bewegungshistorie.
- Bestandsänderung und AuditLog-Eintrag müssen atomar sein.
- Mehrbenutzerfähigkeit und Concurrency-Schutz sind Pflicht.
- Zum Start ausschließlich Administrator; keine öffentliche Selbstregistrierung.
- Optionale Funktionen dürfen den Kernbetrieb nicht blockieren.

## Nächste Schritte
Siehe `ROADMAP.md`. Als Nächstes folgen Supabase-Projekt/Region, SQL-Schema und RLS. Danach Authentifizierung und Admin-Bootstrap, anschließend AuditLog und transaktionale Bestandsfunktionen.
