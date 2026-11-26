# Supabase API-/RPC-Kontrakt

Stand: 26.11.2026

Die Anwendung verwendet die Supabase Data API mit RLS. Direkte Tabellenänderungen sind für `anon` und `authenticated` entzogen. Alle fachlichen Schreibvorgänge laufen über die auditierten PostgreSQL-RPCs.

## Tabellenzugriff

Authentifizierte Administratoren können lesen:
- `articles`
- `locations`
- `stock_items`
- `audit_log`

`profiles` ist für den eigenen Benutzer und Administratoren lesbar. Nutzer ohne Rolle (`role = NULL`) können sich authentifizieren, erhalten aber keinen Inventarzugriff.

## Artikel

Lesezugriffe erfolgen über `public.articles`.

| RPC | Parameter | Ergebnis |
| --- | --- | --- |
| `create_article` | `p_name text, p_category text, p_description text = null, p_manufacturer text = null` | Artikel-JSON |
| `update_article` | `p_article_id uuid, p_name text, p_category text, p_description text = null, p_manufacturer text = null` | aktualisierter Artikel |
| `delete_article` | `p_article_id uuid` | `{ deleted, id }` |

Löschen ist nur möglich, wenn keine aktuellen Materialposten vorhanden sind. Historische Snapshots bleiben erhalten.

## Standorte

Lesezugriffe erfolgen über `public.locations`.

| RPC | Parameter | Ergebnis |
| --- | --- | --- |
| `create_location` | `p_name text, p_description text = null` | Standort-JSON |
| `update_location` | `p_location_id uuid, p_name text, p_description text = null` | aktualisierter Standort |
| `delete_location` | `p_location_id uuid` | `{ deleted, id }` |

Löschen ist nur möglich, wenn keine aktuellen Materialposten vorhanden sind.

## Bestände

Lesezugriffe erfolgen über `public.stock_items`, optional gefiltert nach `location_id`.

Es gibt keinen direkten Endpunkt zum Überschreiben einer Menge.

### Zugang – `receive_stock`

Parameter:
- `p_article_id uuid`
- `p_location_id uuid`
- `p_quantity integer` (positive ganze Zahl)
- `p_expiration_date date = null`
- `p_batch_number text = null`
- `p_manufacturer text = null`
- `p_note text = null`

Erhöht einen exakt passenden Materialposten oder legt einen neuen an. Ergebnis enthält `stock_item` und `created`.

### Verschiebung – `transfer_stock`

Parameter:
- `p_stock_item_id uuid`
- `p_target_location_id uuid`
- `p_quantity integer` (positive ganze Zahl)
- `p_note text = null`

Reduziert oder löscht den Quellposten und erhöht/legt den exakt passenden Zielposten an. Quelle und Ziel werden in einem AuditLog-Eintrag zusammengefasst.

### Sammelentnahme – `remove_stock_batch`

Parameter:
- `p_source_location_id uuid`
- `p_items jsonb`
- `p_note text = null`

`p_items` ist ein nicht-leeres Array konkreter Materialposten und Mengen:

~~~json
[
  { "stockItemId": "00000000-0000-0000-0000-000000000003", "quantity": 2 }
]
~~~

Jede UUID muss zum Quellstandort gehören. Doppelte Positionen und Mengen größer als der verfügbare Bestand werden abgelehnt. Ein vollständig entnommener Posten wird gelöscht. Die gesamte Aktion erzeugt einen AuditLog-Eintrag.

## Zusammenführung und Identität

Die aktuelle technische Ausgangsregel verlangt exakte Übereinstimmung aller Identitätsmerkmale: Artikel, Zielstandort, Ablaufdatum, Charge und Hersteller. NULL und bekannte Werte gelten als verschieden. Diese konservative Regel verhindert ungewollte Vermischung, bis die offenen fachlichen Fragen endgültig bestätigt wurden.

## Sicherheit und Transaktionsverhalten

- Jede Schreib-RPC prüft selbst die Datenbankrolle `administrator`.
- SECURITY DEFINER-Funktionen verwenden `search_path = ''`, qualifizierte Objektnamen und explizite EXECUTE-Rechte.
- Die Datenbank schreibt pro erfolgreicher Schreib-RPC genau einen AuditLog-Eintrag.
- Die fachliche Mutation und AuditLog-INSERT laufen in derselben PostgreSQL-Transaktion; ein Fehler rollt beides zurück.
- Es gibt genau eine Tabelle `audit_log`, keine `stock_movements`-Tabelle.

Fehler werden als PostgreSQL-/PostgREST-Fehler zurückgegeben. Die UI muss SQLSTATEs auf verständliche Meldungen abbilden (z. B. `42501` Berechtigung, `P0002` nicht gefunden, `22023` ungültige Eingabe, `23503` abhängige Bestände).
