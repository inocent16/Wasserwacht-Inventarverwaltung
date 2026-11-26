# Frontend

SolidJS-Frontend auf Basis von TanStack Start, TanStack Router und TanStack Query. Supabase Cloud stellt PostgreSQL und Auth bereit.

## Voraussetzungen
- Node.js 22 oder neuer
- npm
- Zugriff auf das Supabase-Projekt `wasserwacht-inventarverwaltung`

## Lokal starten
1. `.env.example` nach `.env.local` kopieren (die Datei enthält die öffentliche Konfiguration des Projekt-Backends).
2. Bei Bedarf `VITE_SUPABASE_URL` und `VITE_SUPABASE_PUBLISHABLE_KEY` aus dem Supabase-Projekt aktualisieren.
3. Abhängigkeiten installieren und den Entwicklungsserver starten:

~~~bash
npm install
npm run dev
~~~

Die Anwendung läuft unter http://localhost:3000. Die Supabase-Clients werden erst bei ihrer Verwendung erzeugt.

## Technologie
- SolidJS 1.9 (stabile Kompatibilitätsbasis für TanStack Start 1.x)
- TanStack Start und TanStack Router
- TanStack Query für Server-State
- Supabase JS und `@supabase/ssr` für Supabase- und Cookie-Session-Integration
- TypeScript und Zod
- Generierte Cloud-Datenbanktypen unter `src/lib/supabase/database.types.ts`

## Sicherheit
- Nur die Supabase Project URL und der Publishable Key dürfen im Browser verfügbar sein.
- Keine Secret- oder Service-Role-Keys mit einem `VITE_`-Präfix versehen oder ins Frontend geben.
- Private Serverzugriffe gehören in Server Functions oder Server Routes.
- Authentifizierte Datenbanktabellen erhalten Row Level Security (RLS).
- Serverseitige Authentifizierung muss den Benutzer mit `auth.getUser()` verifizieren; ein ungeprüfter `getSession()`-Wert ist keine Autorisierungsentscheidung.

## Aktueller Umfang
Das TanStack-Start-Grundgerüst, E-Mail-/Passwort-Login-UI, Logout und Session-Prüfung sind vorhanden. Die Supabase-Clients sind request-/browser-spezifisch und anhand des Cloud-Schemas typisiert. Geschützte Navigation, Admin-Einladung, Inventaroberfläche und End-to-End-Tests stehen noch aus.

Die Cloud-Self-Signup-Einstellung muss vor dem ersten Benutzer manuell deaktiviert werden: Supabase Dashboard → Authentication → General Configuration → **Allow new users to sign up**.
