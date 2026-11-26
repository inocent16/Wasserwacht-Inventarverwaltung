# Frontend

SolidJS-Frontend auf Basis von TanStack Start, TanStack Router und TanStack Query. Supabase Cloud stellt PostgreSQL und Auth bereit.

## Voraussetzungen
- Node.js 22 oder neuer
- npm
- Ein Supabase-Cloud-Projekt (für Auth- und Datenbanktests)

## Lokal starten
1. `.env.example` nach `.env.local` kopieren.
2. `VITE_SUPABASE_URL` und `VITE_SUPABASE_PUBLISHABLE_KEY` aus dem Supabase-Projekt eintragen.
3. Abhängigkeiten installieren und den Entwicklungsserver starten:

~~~bash
npm install
npm run dev
~~~

Die Anwendung läuft unter http://localhost:3000. Für reine UI-Arbeit kann sie auch ohne Supabase-Zugangsdaten starten; Supabase-Clients werden erst bei ihrer Verwendung erzeugt.

## Technologie
- SolidJS 1.9 (stabile Kompatibilitätsbasis für TanStack Start 1.x)
- TanStack Start und TanStack Router
- TanStack Query für Server-State
- Supabase JS und `@supabase/ssr` für Supabase- und Cookie-Session-Integration
- TypeScript und Zod

## Sicherheit
- Nur die Supabase Project URL und der Publishable Key dürfen im Browser verfügbar sein.
- Keine Secret- oder Service-Role-Keys mit einem `VITE_`-Präfix versehen oder ins Frontend geben.
- Private Serverzugriffe gehören in Server Functions oder Server Routes.
- Authentifizierte Datenbanktabellen erhalten Row Level Security (RLS).
- Serverseitige Authentifizierung muss den Benutzer mit `auth.getUser()` verifizieren; ein ungeprüfter `getSession()`-Wert ist keine Autorisierungsentscheidung.

## Aktueller Umfang
Das TanStack-Start-Grundgerüst und getrennte Supabase-Clients für Browser und Server sind angelegt. Login, Benutzerverwaltung, Datenbankschema und Inventarfunktionen sind noch nicht implementiert. Die Login-Kennung (E-Mail oder ein abgeleiteter Benutzername) wird vor der Auth-Implementierung festgelegt.
