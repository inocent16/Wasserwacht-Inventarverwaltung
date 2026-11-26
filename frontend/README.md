# Frontend

React- und TypeScript-Webclient mit Vite. Die erste Oberfläche stellt die Administrator-Anmeldung bereit und kommuniziert mit der ASP.NET-Core-API.

## Lokal starten

Voraussetzungen: Node.js 22 oder neuer sowie ein laufendes Backend mit PostgreSQL-Konfiguration.

1. Backend mit HTTPS starten (Standardziel des Vite-Proxys: `https://localhost:7001`).
2. Optional `.env.example` nach `.env.local` kopieren und `WASSERWACHT_API_TARGET` anpassen.
3. Abhängigkeiten installieren und Entwicklungsserver starten:

```bash
npm install
npm run dev
```

Vite stellt die Oberfläche standardmäßig unter `http://localhost:5173` bereit und leitet `/api` sowie `/health` an das Backend weiter. Für die lokale Entwicklung muss das ASP.NET-HTTPS-Entwicklungszertifikat vom System als vertrauenswürdig erkannt werden; der Proxy akzeptiert das lokale Zertifikat.

## Anmeldung

- Holt vor zustandsändernden Anfragen ein Antiforgery-Token über `GET /api/v1/auth/csrf`.
- Sendet das Token im Header `X-CSRF-TOKEN`; Session-Cookie bleibt HttpOnly und wird nicht aus JavaScript gelesen.
- Prüft den angemeldeten Benutzer über `GET /api/v1/auth/me`.
- Unterstützt Anmeldung und Abmeldung über die Auth-API.
- Das Frontend speichert weder Passwort noch Session-Cookie selbst.

Die Oberfläche ist derzeit bewusst auf die Anmeldung und einen geschützten Platzhalter nach dem Login begrenzt. Artikel, Standorte, Bestände und Bewegungsaktionen werden in den nächsten Schritten ergänzt. Fachliche Bestandslogik bleibt ausschließlich im Backend.

## Build

```bash
npm run build
```
