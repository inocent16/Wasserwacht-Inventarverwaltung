# Entwicklerstart

## Voraussetzungen
- Node.js 22 oder neuer
- npm
- Supabase-Cloud-Projekt für Auth- und Datenbanktests
- Git

## Frontend starten
1. `frontend/.env.example` nach `frontend/.env.local` kopieren.
2. Supabase Project URL und Publishable Key aus dem Supabase-Dashboard eintragen.
3. Im Frontend-Verzeichnis ausführen:

~~~bash
cd frontend
npm install
npm run dev
~~~

Die Anwendung ist anschließend unter http://localhost:3000 erreichbar.

Für reine UI-Arbeit können die Zugangsdaten zunächst leer bleiben. Sobald Supabase-Funktionen aufgerufen werden, müssen URL und Publishable Key konfiguriert sein.

## Supabase Cloud vorbereiten
1. Ein Supabase-Projekt in der gewünschten EU-Region erstellen (Frankfurt ist bevorzugt, aber noch nicht festgelegt).
2. Project URL und Publishable Key aus dem API-/Connect-Bereich übernehmen.
3. SQL-Schemaänderungen ausschließlich als versionierte Dateien unter `supabase/migrations/` ablegen und über Supabase CLI anwenden.
4. RLS-Policies für jede exponierte Tabelle prüfen und durch Tests absichern.

Das Repository ist noch nicht mit einer konkreten Supabase-Instanz verbunden. Es gibt daher noch keine produktive Datenbank oder Zugangsdaten im Repository.

## Supabase Auth und erster Administrator

Die Anwendung verwendet E-Mail-Adresse und Passwort. Öffentliche Selbstregistrierung muss im Supabase-Dashboard unter Authentication > Sign In / Providers bzw. den Auth-URL-/Signup-Einstellungen deaktiviert werden; der E-Mail-Provider für die Anmeldung muss dabei aktiviert bleiben.

Nach dem Anwenden der Migration `20261126233000_profiles_and_admin_role.sql` wird das erste Konto kontrolliert angelegt:

1. Im Supabase-Dashboard unter Authentication > Users das erste Konto manuell anlegen (E-Mail und initiales Passwort; E-Mail-Bestätigung passend zur Betriebsentscheidung konfigurieren).
2. Im SQL Editor die Profilrolle gezielt für genau diese E-Mail freischalten:

~~~sql
update public.profiles p
set role = 'administrator', updated_at = now()
from auth.users u
where p.id = u.id
  and lower(u.email) = lower('erste-admin-adresse@example.org');
~~~

3. Prüfen, dass genau eine Zeile geändert wurde. Keine öffentliche Registrierung aktivieren, um weitere Konten anzulegen.

Das Profil-Trigger legt neue Profile absichtlich mit `role = NULL` an. Solche Konten können sich zwar authentifizieren, erhalten aber keinen Zugriff auf die Inventarverwaltung. Die Admin-Einladungsfunktion wird später serverseitig mit einem Supabase Secret Key implementiert; dieser Schlüssel darf niemals im Browser verfügbar sein.

## Sicherheitsregeln
- Keine echten Zugangsdaten oder API-Secrets committen.
- Publishable Key darf im Browser verwendet werden; er ersetzt keine RLS-Policy.
- Service-Role-/Secret-Keys niemals mit `VITE_`-Präfix versehen.
- Admin-API-Aufrufe dürfen nur serverseitig erfolgen.
- Autorisierungsentscheidungen auf dem Server müssen den Benutzer gegen Supabase Auth verifizieren.
- Authentifizierte Daten nicht in gemeinsam genutzten SSR-Caches legen.

## Übergangsbestand
Das Verzeichnis `backend/` enthält das bisherige ASP.NET-Core-/EF-Core-Grundgerüst. Es bleibt während der Migration als Referenz und Regression-Baseline erhalten. Es ist nicht mehr die Zielarchitektur und wird erst gelöscht, wenn Datenbankschema, Authentifizierung, AuditLog und Inventarfunktionen auf Supabase vollständig umgesetzt und getestet sind.

## Tests
- Frontend: `npm run typecheck` und `npm run build`.
- Datenbank: Migrationstests gegen PostgreSQL / Supabase-Testumgebung.
- Sicherheit: RLS-Tests für anon, authenticated und Administrator.
- Bestand: Integrationstests für Mengenvalidierung, Concurrency, Rollback und exakt einen AuditLog-Eintrag pro erfolgreicher Aktion.
