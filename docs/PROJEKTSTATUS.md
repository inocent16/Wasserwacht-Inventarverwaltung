# Projektstatus und Entwicklerübergabe

Stand: 27.11.2026
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

## Supabase Cloud – angelegt und verbunden
- Projekt: `wasserwacht-inventarverwaltung`
- Projekt-Ref: `mqzdlvvzawaoidgajuho`
- Organisation: `inno16's org`
- Region: Frankfurt / EU Central (`eu-central-1`)
- API-URL: `https://mqzdlvvzawaoidgajuho.supabase.co`
- Status zuletzt geprüft: `ACTIVE_HEALTHY`
- Vier versionierte Migrationen erfolgreich auf Cloud-PostgreSQL angewendet
- Fünf Tabellen vorhanden: `profiles`, `articles`, `locations`, `stock_items`, `audit_log`
- Datenbank ist aktuell leer; es wurden keine Test- oder Produktdaten angelegt
- RLS ist auf allen fünf Tabellen aktiviert
- Rechteprüfung bestätigt: `service_role` hat keine direkten Tabellenrechte und darf die Bestands-RPCs nicht ausführen; `anon` darf keine Bestands-RPCs ausführen; authentifizierte Benutzer können die geprüften RPCs aufrufen, die intern Administratorrechte verlangen
- Health Advisor: keine Findings
- Security Advisor meldet zehn SECURITY DEFINER-Funktionen, die für `authenticated` ausführbar sind. Das ist für die vorgesehenen RPCs beabsichtigt; jede Schreibfunktion prüft intern die Datenbankrolle `administrator`. Vor Produktivfreigabe nochmals reviewen.
- Performance Advisor meldet sechs ungenutzte Indizes (INFO), erwartbar bei einer leeren Datenbank; nach realer Nutzung erneut prüfen.
- TypeScript-Datenbanktypen aus dem Cloud-Schema generiert und im Frontend abgelegt.

**Wichtig – Auth-Konfiguration:** Die lokale Datei `supabase/config.toml` deaktiviert öffentliche Registrierung für lokale Supabase-Umgebungen. Diese Einstellung ist nicht automatisch durch SQL-Migrationen auf das Cloud-Projekt übertragen. Im Cloud-Dashboard muss unter Authentication → General Configuration die Option **Allow new users to sign up** deaktiviert werden. Dieser manuelle Cloud-Schritt ist noch zu bestätigen. Noch keinen Benutzer anlegen, bevor die Registrierung deaktiviert ist.

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
- direkte Tabellenrechte auch für `service_role` entzogen
- auditiertes RPC für Artikel- und Standort-CRUD
- transaktionale RPCs für Zugang, Verschiebung und Sammelentnahme
- advisory-/row-level concurrency locks und Unique Identity Index
- append-only Schutz für das zentrale AuditLog
- Supabase CLI-Konfiguration mit deaktivierter öffentlicher Registrierung für lokale Umgebung
- pgTAP-Schema-, RLS-, Rollen- und Bestandsablauf-Tests
- Database-CI: frische lokale Supabase-Instanz startet, alle vier Migrationen werden angewendet und die pgTAP-Suite besteht
- Frontend-CI: Vite Build und TypeScript-Prüfung erfolgreich
- Cloud-Projekt erstellt, vier Migrationen remote angewendet und Tabellen/RLS/Rechte geprüft
- generierte Supabase-TypeScript-Typen in Browser- und Server-Client integriert
- Frontend-.env.example mit Cloud-URL und Publishable Key versehen (beide sind öffentliche Client-Konfiguration)
- Architektur- und Entwicklerdokumentation aktualisiert

## Noch nicht vorhanden / noch zu verifizieren
- Cloud-Einstellung „Allow new users to sign up“ deaktivieren und Zustand bestätigen
- erstes Administratorkonto im Auth-Dashboard anlegen und Profilrolle einmalig freischalten
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
1. Cloud-Self-Signup im Dashboard deaktivieren und bestätigen.
2. Erstes Auth-Konto erstellen und `profiles.role = 'administrator'` einmalig freischalten.
3. Login gegen Cloud testen.
4. Admin-Einladungsfunktion und geschützte Navigation implementieren.
5. Inventaroberfläche und End-to-End-Tests ergänzen.
