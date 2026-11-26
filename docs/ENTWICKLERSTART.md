# Entwicklerstart

## Voraussetzungen
- Node.js 22 oder neuer
- npm
- Docker Desktop / Docker Engine (für lokale Supabase-Datenbank und pgTAP-Tests)
- Supabase CLI
- Git

## Frontend starten
1. `frontend/.env.example` nach `frontend/.env.local` kopieren.
2. Project URL und Publishable Key aus dem Supabase-Dashboard oder der lokalen CLI eintragen.
3. Im Frontend-Verzeichnis ausführen:

~~~bash
cd frontend
npm install
npm run dev
~~~

Die Anwendung läuft unter http://localhost:3000.

## Supabase lokal starten
Im Repository-Root:

~~~bash
supabase start
supabase status
~~~

Die lokale Startumgebung verwendet die Einstellungen aus `supabase/config.toml`. Signup ist dort global und für den E-Mail-Provider deaktiviert. `supabase start` wendet die versionierten SQL-Migrationen aus `supabase/migrations/` auf einer frischen lokalen Datenbank an.

Die Werte `API URL` und `Publishable key` aus `supabase status` in `frontend/.env.local` übernehmen:

~~~dotenv
VITE_SUPABASE_URL=http://127.0.0.1:54321
VITE_SUPABASE_PUBLISHABLE_KEY=<lokaler-publishable-key>
~~~

Lokale Datenbank zurücksetzen und alle Migrationen neu anwenden:

~~~bash
supabase db reset
~~~

pgTAP-Datenbanktests ausführen:

~~~bash
supabase test db
~~~

Die GitHub-Actions-Workflowdatei `.github/workflows/database.yml` führt dieselben Migrationen und pgTAP-Tests automatisiert in einem lokalen Supabase-Stack aus.

## Supabase Cloud verbinden
1. Ein Cloud-Projekt in der gewünschten EU-Region erstellen (Frankfurt ist bevorzugt, aber noch nicht festgelegt).
2. Im Dashboard unter Authentication die öffentliche Registrierung deaktivieren; den E-Mail-Provider für die Anmeldung aktiviert lassen.
3. Project URL und Publishable Key nach `frontend/.env.local` übernehmen.
4. Projekt-Referenz und DB-Passwort nicht committen. Lokal im Repository-Root verbinden:

~~~bash
supabase login
supabase link --project-ref <project-ref>
supabase db push
~~~

Vor `supabase db push` Zielprojekt und Migrationen prüfen. Zugangsdaten und Projekt-Referenzen bleiben außerhalb des Git-Repositories.

## Ersten Administrator anlegen
Die Authentifizierung erfolgt mit E-Mail-Adresse und Passwort. Neue Auth-Profile werden vom Datenbanktrigger zunächst mit `role = NULL` angelegt.

1. Im Supabase-Dashboard unter Authentication > Users das erste Konto manuell anlegen. Die E-Mail-Adresse muss bestätigt sein, damit die Anmeldung wie konfiguriert funktioniert.
2. Im SQL Editor genau dieses Konto freischalten:

~~~sql
update public.profiles p
set role = 'administrator', updated_at = now()
from auth.users u
where p.id = u.id
  and lower(u.email) = lower('erste-admin-adresse@example.org');
~~~

3. Prüfen, dass genau eine Zeile geändert wurde.
4. Keine öffentliche Registrierung aktivieren, um weitere Konten anzulegen.

Weitere Benutzer werden künftig ausschließlich über eine geschützte serverseitige Administrator-Einladungsfunktion provisioniert. Diese Funktion ist noch nicht implementiert.

## Datenbankzugriff und Sicherheit
- RLS ist für alle Anwendungstabellen aktiviert.
- Publishable Key darf im Browser verwendet werden; er ersetzt keine RLS-Policy.
- Service-Role-/Secret-Keys niemals mit `VITE_`-Präfix versehen.
- Direkte Tabellen-DML-Rechte für anon/authenticated sind entzogen; fachliche Änderungen laufen über auditiertes RPC.
- SECURITY DEFINER-Funktionen prüfen die Administratorrolle in PostgreSQL und setzen einen leeren `search_path`.
- Keine echten Passwörter, Tokens oder Secrets committen.

## Übergangsbestand
Das Verzeichnis `backend/` enthält das bisherige ASP.NET-Core-/EF-Core-Grundgerüst. Es bleibt während der Migration als Referenz erhalten und wird erst entfernt, wenn Schema, Authentifizierung, AuditLog und Inventarfunktionen auf Supabase vollständig getestet sind.
