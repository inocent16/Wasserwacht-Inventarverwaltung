# Projektstatus und Entwicklerübergabe

Stand: 26.11.2026
Ziel: Version 1.0

## Kurzüberblick
Die Zielarchitektur ist SolidJS + TanStack Start + Supabase Cloud. Das bisherige C#-Backend bleibt während der Migration als Referenz erhalten.

## Zieltechnologie
- SolidJS 1.9
- TanStack Start 1.x / TanStack Router
- TanStack Query 5.x
- Supabase Cloud: PostgreSQL und Auth
- `@supabase/ssr` für Cookie-basierte SSR-Sitzungen
- TypeScript und Zod
- SQL-Migrationen und pgTAP-Tests mit Supabase CLI

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
- TanStack Start, TanStack Router und TanStack Query eingerichtet
- Supabase Browser- und Server-Client mit `@supabase/ssr`
- E-Mail-/Passwort-Login, Logout und serverseitige Session-Prüfung über Supabase Auth
- Profile-/Administrator-Migration; neue Profile erhalten standardmäßig keine Rolle
- SQL-Tabellen für Artikel, Standorte, Materialposten und zentrale AuditLog-Historie
- RLS-Policies und read-only Tabellenrechte für API-Rollen
- auditiertes RPC für Artikel- und Standort-CRUD
- transaktionale RPCs für Zugang, Verschiebung und Sammelentnahme
- advisory-/row-level concurrency locks und Unique Identity Index
- append-only Schutz für das zentrale AuditLog
- Supabase CLI-Konfiguration mit deaktivierter öffentlicher Registrierung
- pgTAP-Schema-, RLS-, Rollen- und Bestandsablauf-Tests
- Database-CI: frische lokale Supabase-Instanz startet, alle drei Migrationen werden angewendet und die pgTAP-Suite besteht
- Frontend-CI: Vite Build und TypeScript-Prüfung erfolgreich
- Architektur- und Entwicklerdokumentation aktualisiert

## Noch nicht vorhanden / noch zu verifizieren
- Supabase-Cloud-Projekt, Region und konfigurierte Secrets
- Ausführung der Migrationen gegen eine echte Cloud-Instanz
- echte parallele Concurrency-/Race-Integrationstests (bisher Sperrmechanismen und Constraints implementiert, Bestandsabläufe sequenziell getestet)
- Admin-Einladungsfunktion und Passwortänderung/-zurücksetzung
- geschützte Anwendungsnavigation und vollständige Inventaroberfläche
- Deployment-Konfiguration und End-to-End-Tests
- Backup-/Restore-Test

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
- Keine automatische Standortanlage, keine Standorttypen und kein fest eingebauter Standort „Lager“.
- Artikel ohne Ablaufdatum werden unterstützt.
- Bestände werden bei Zugang und Verschiebung nur mit exakt gleicher Artikel-/Standort-/Ablaufdatum-/Charge-/Hersteller-Identität zusammengeführt; NULL wird nicht mit einem bekannten Wert vermischt.
- Vollständig entnommene Posten werden gelöscht, Historieneinträge bleiben.
- Genau eine zentrale AuditLog-Tabelle, genau ein Eintrag pro erfolgreicher fachlicher Aktion.
- Es gibt keine StockMovement-Tabelle und keine separate Bewegungshistorie.
- Bestandsänderung und AuditLog-Eintrag müssen atomar sein.
- Mehrbenutzerfähigkeit und Concurrency-Schutz sind Pflicht.
- Zum Start ausschließlich Administrator; keine öffentliche Selbstregistrierung.
- Optionale Funktionen dürfen den Kernbetrieb nicht blockieren.

## Nächste Schritte
Siehe `ROADMAP.md`. Als Nächstes Supabase-Cloud-Projekt/Region einrichten und Migrationen nach Review dort anwenden. Danach Admin-Einladungsfunktion, geschützte Navigation und Inventaroberfläche umsetzen.
