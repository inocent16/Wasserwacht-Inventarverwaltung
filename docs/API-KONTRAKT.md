# API-Kontrakt – Vorschlag für Version 1

Status: Entwurf für die Implementierung, noch nicht durch OpenAPI bestätigt.

Basispräfix: `/api/v1`

Die API ist die fachliche Grenze zwischen Webclient und Backend. Bestandsänderungen erfolgen ausschließlich über Bewegungsendpunkte, nicht über direkte Mengenänderungen an Materialposten.

## Artikel

- `GET /api/v1/articles` – Artikelliste
- `POST /api/v1/articles` – Artikel anlegen
- `GET /api/v1/articles/{articleId}` – Artikeldetails
- `PUT /api/v1/articles/{articleId}` – Artikel ändern
- `DELETE /api/v1/articles/{articleId}` – Löschen nur, wenn keine abhängigen Bestände oder Historieneinträge entgegenstehen; endgültige Regel noch festzulegen

Beispiel Anlegen:
```json
{
  "name": "NaCl 500 ml",
  "category": "Infusion",
  "description": null,
  "manufacturer": null
}
```

## Standorte

- `GET /api/v1/locations`
- `POST /api/v1/locations`
- `GET /api/v1/locations/{locationId}`
- `PUT /api/v1/locations/{locationId}`
- `DELETE /api/v1/locations/{locationId}`

Es wird kein Standort automatisch erzeugt. Löschregeln für Standorte mit Beständen oder Historieneinträgen sind noch festzulegen.

## Bestände

- `GET /api/v1/stock-items?locationId={id}` – aktuelle Materialposten, optional nach Standort gefiltert
- `GET /api/v1/stock-items/{stockItemId}` – einzelner aktueller Materialposten

Es gibt bewusst keinen allgemeinen Endpunkt zum direkten Überschreiben einer Menge.

## Bewegungen

- `POST /api/v1/movements/incoming` – Zugang
- `POST /api/v1/movements/transfer` – Verschiebung
- `POST /api/v1/movements/removal-batch` – Sammelentnahme
- `GET /api/v1/movements` – Historie, Filter und Pagination sind zu spezifizieren

Beispiel Zugang:
```json
{
  "articleId": "00000000-0000-0000-0000-000000000001",
  "locationId": "00000000-0000-0000-0000-000000000002",
  "quantity": 12,
  "expirationDate": "2028-06-01",
  "batchNumber": "CH-123",
  "manufacturer": null,
  "note": null
}
```

Beispiel Sammelentnahme:
```json
{
  "sourceLocationId": "00000000-0000-0000-0000-000000000002",
  "items": [
    {
      "stockItemId": "00000000-0000-0000-0000-000000000003",
      "quantity": 2
    }
  ],
  "note": "Materialverbrauch"
}
```

## Allgemeine Regeln

- IDs sind UUIDs.
- Mengen sind positive Ganzzahlen in Stück.
- Datumswerte für Ablaufdaten sind ISO-8601-Datum (`YYYY-MM-DD`), ohne Uhrzeit.
- Ungültige Anfragen liefern strukturierte 400-Fehler.
- Nicht gefundene IDs liefern 404.
- Nicht ausreichender Bestand liefert 409 Conflict.
- Alle Änderungen einer Bewegung und deren Historieneintrag werden in einer Transaktion gespeichert.
- Der Server berechnet und prüft Bestände; Clientwerte sind nicht vertrauenswürdig.

Die endgültigen DTOs, Fehlerstruktur, Sortierung und Pagination sind bei der Implementierung in OpenAPI festzuhalten.
